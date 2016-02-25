// -----------------------------------------------------------------------
//  <copyright file="RelayManager.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;

    using OutbreakLabs.LibPacketGremlin.Abstractions;
    using OutbreakLabs.LibPacketGremlin.Extensions;
    using OutbreakLabs.LibPacketGremlin.Packets;
    using OutbreakLabs.LibPacketGremlin.Packets.IPv4Support;

    using SharpPcap;

    /// <summary>
    ///     Manages routes and adapters to rewrite and relay broadcast packets
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class RelayManager : IDisposable
    {
        private readonly IPAddress[] localAddresses;

        private readonly List<NetworkAdapterMonitor> networkAdapterMonitors;

        private readonly HashSet<IPAddress> destinations;

        private readonly ReaderWriterLockSlim destinationsLock;

        private readonly IUdpSender udpSender;

        private long packetsRelayed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayManager" /> class, automatically detecting local addresses.
        /// </summary>
        public RelayManager()
            : this(GetLocalAddresses(), new UdpClientSender())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayManager" /> class.
        /// </summary>
        /// <param name="localAddresses">Local IP addresses (used for filtering, so only local packets are relayed)</param>
        /// <exception cref="System.ArgumentNullException">localAddresses must not be null</exception>
        public RelayManager(IPAddress[] localAddresses, IUdpSender udpSender)
        {
            if (localAddresses == null)
            {
                throw new ArgumentNullException(nameof(localAddresses));
            }

            if (udpSender == null)
            {
                throw new ArgumentNullException(nameof(udpSender));
            }

            this.networkAdapterMonitors = new List<NetworkAdapterMonitor>();
            this.localAddresses = localAddresses;
            this.destinations = new HashSet<IPAddress>();
            this.destinationsLock = new ReaderWriterLockSlim();
            this.udpSender = udpSender;
        }

        /// <summary>
        ///     Gets the total number of packets relayed
        /// </summary>
        public long PacketsRelayed => Interlocked.Read(ref this.packetsRelayed);

        private static IPAddress[] GetLocalAddresses()
        {
            return
                NetworkInterface.GetAllNetworkInterfaces()
                    .Where(i => i.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                    .Where(i => i.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(i => i.Address)
                    .ToArray();
        }

        /// <summary>
        ///     Enables monitoring of a capture device for broadcast packets
        /// </summary>
        /// <param name="captureDevice">Capture device to monitor</param>
        /// <exception cref="System.ArgumentNullException">captureDevice must not be null</exception>
        public void EnableCaptureDevice(ICaptureDevice captureDevice)
        {
            if (captureDevice == null)
            {
                throw new ArgumentNullException(nameof(captureDevice));
            }

            if (!this.networkAdapterMonitors.Any(m => m.CaptureDevice == captureDevice))
            {
                this.networkAdapterMonitors.Add(
                    new NetworkAdapterMonitor(captureDevice, this.localAddresses, this.PacketReceived));
            }
        }

        /// <summary>
        ///     Disables monitoring of a capture device
        /// </summary>
        /// <param name="captureDevice">The capture device.</param>
        /// <exception cref="System.ArgumentNullException">captureDevice must not be null</exception>
        public void DisableCaptureDevice(ICaptureDevice captureDevice)
        {
            if (captureDevice == null)
            {
                throw new ArgumentNullException(nameof(captureDevice));
            }

            var monitor = this.networkAdapterMonitors.SingleOrDefault(m => m.CaptureDevice == captureDevice);
            if (monitor != null)
            {
                monitor.Dispose();
                this.networkAdapterMonitors.Remove(monitor);
            }
        }

        /// <summary>
        ///     Adds a destination for rewritten broadcast packets
        /// </summary>
        /// <param name="destination">The destination to add</param>
        public void AddDestination(IPAddress destination)
        {
            try
            {
                this.destinationsLock.EnterWriteLock();
                this.destinations.Add(destination);
            }
            finally
            {
                this.destinationsLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     Removes a destination for rewritten broadcast packets
        /// </summary>
        /// <param name="destination">The destination to remove</param>
        public void RemoveDestination(IPAddress destination)
        {
            try
            {
                this.destinationsLock.EnterWriteLock();
                this.destinations.Remove(destination);
            }
            finally
            {
                this.destinationsLock.ExitWriteLock();
            }
        }

        private void PacketReceived(IPacket pdata)
        {
            IPAddress[] destinationsCopy;
            try
            {
                this.destinationsLock.EnterReadLock();
                destinationsCopy = this.destinations.ToArray();
            }
            finally
            {
                this.destinationsLock.ExitReadLock();
            }

            foreach (var destination in destinationsCopy)
            {
                IPv4<UDP> ip = (IPv4<UDP>)pdata.Layer<IPv4>();
                ip.DestAddress = new IPv4Address(destination);
                ip.CorrectFields();
                this.udpSender.Send(ip);
                Interlocked.Increment(ref this.packetsRelayed);
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (var monitor in this.networkAdapterMonitors)
                    {
                        monitor.Dispose();
                    }
                    this.networkAdapterMonitors.Clear();
                    this.destinationsLock.Dispose();
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        #endregion
    }
}