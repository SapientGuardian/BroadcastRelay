// -----------------------------------------------------------------------
//  <copyright file="NetworkAdapterMonitor.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BroadcastRelayCoreTests")]

namespace OutbreakLabs.BroadcastRelay.Core
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reactive.Linq;
    
    using OutbreakLabs.LibPacketGremlin.Abstractions;
    using OutbreakLabs.LibPacketGremlin.Extensions;
    using OutbreakLabs.LibPacketGremlin.PacketFactories;
    using OutbreakLabs.LibPacketGremlin.Packets;

    using SharpPcap;

    /// <summary>
    ///     Monitors and filters a single adapter for broadcast packets
    /// </summary>
    /// <seealso cref="System.IDisposable" />    
    internal class NetworkAdapterMonitor : IDisposable
    {
        private readonly IDisposable subscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NetworkAdapterMonitor" /> class.
        /// </summary>
        /// <param name="captureDevice">The capture device to monitor.</param>
        /// <param name="localAddresses">The local addresses to filter against.</param>
        /// <param name="packetCallback">The callback for matching packets.</param>
        public NetworkAdapterMonitor(
            ICaptureDevice captureDevice,
            IPAddress[] localAddresses,
            Action<IPacket> packetCallback)
        {
            var obs =
                Observable.FromEventPattern<PacketArrivalEventHandler, CaptureEventArgs>(
                    ev => captureDevice.OnPacketArrival += ev,
                    ev => captureDevice.OnPacketArrival -= ev);

            var udp4BcastPackets = from sharpPacketEvent in obs
                                   let parsed =
                                       sharpPacketEvent.EventArgs.Packet.LinkLayerType
                                       == PacketDotNet.LinkLayers.Ethernet
                                           ? (IPacket)EthernetIIFactory.Instance.ParseAs(sharpPacketEvent.EventArgs.Packet.Data)
                                           : (IPacket)GenericFactory.Instance.ParseAs(sharpPacketEvent.EventArgs.Packet.Data)             
                                   let layers = parsed?.Layers() ?? Enumerable.Empty<IPacket>()
                                   let ipv4 = layers.OfType<IPv4>().FirstOrDefault()
                                   where
                                       layers.OfType<UDP>().Any() && layers.OfType<IPv4>().Any()
                                       && ipv4.DestAddress.GetAddressBytes().Last() == 255
                                       && localAddresses.Any(la => la.Equals(ipv4.SourceAddress))
                                   select parsed;

            this.CaptureDevice = captureDevice;
            this.subscription = udp4BcastPackets.Subscribe(packetCallback);
            captureDevice.Open();
            captureDevice.StartCapture();
        }

        /// <summary>
        ///     Gets the network adapter monitored by this instance.
        /// </summary>
        /// <value>
        ///     The network adapter.
        /// </value>
        public ICaptureDevice CaptureDevice { get; }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.subscription.Dispose();
                    this.CaptureDevice.StopCapture();
                    this.CaptureDevice.Close();
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