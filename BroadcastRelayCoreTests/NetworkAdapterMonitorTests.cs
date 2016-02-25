namespace OutbreakLabs.BroadcastRelayCoreTests
{
    using System;
    using System.Net;

    using Moq;

    using OutbreakLabs.BroadcastRelay.Core;
    using OutbreakLabs.LibPacketGremlin.Abstractions;
    using OutbreakLabs.LibPacketGremlin.Extensions;
    using OutbreakLabs.LibPacketGremlin.PacketFactories;
    using OutbreakLabs.LibPacketGremlin.Packets.IPv4Support;

    using SharpPcap;

    using Xunit;
    using System.Linq;

    public class NetworkAdapterMonitorTests
    {
        [Fact]
        public void OpensAndStartsCapture()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var callback = new Mock<Action<IPacket>>();

            var monitor = new NetworkAdapterMonitor(captureDevice.Object, localAddresses, callback.Object);

            captureDevice.Verify(cd => cd.Open(), Times.Once);
            captureDevice.Verify(cd => cd.StartCapture(), Times.Once);
        }

        [Fact]
        public void StopsCaptureOnDispose()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var callback = new Mock<Action<IPacket>>();

            var monitor = new NetworkAdapterMonitor(captureDevice.Object, localAddresses, callback.Object);

            monitor.Dispose();
            captureDevice.Verify(cd => cd.StopCapture(), Times.Once);
            
        }

        [Fact]
        public void PerformsCallbackForLocalBroadcast()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var callback = new Mock<Action<IPacket>>();

            var monitor = new NetworkAdapterMonitor(captureDevice.Object, localAddresses, callback.Object);


            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));            

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.2");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");

            localBroadcast.CorrectFields();

            byte[] packetData = localBroadcast.ToArray();
            
            PublishPacket(captureDevice, packetData);

            callback.Verify(cb => cb(It.Is<IPacket>(p=>p.ToArray().SequenceEqual(localBroadcast.ToArray()))), Times.Once);
        }

        [Fact]
        public void DoesNotPerformCallbackForNonLocalBroadcast()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var callback = new Mock<Action<IPacket>>();

            var monitor = new NetworkAdapterMonitor(captureDevice.Object, localAddresses, callback.Object);


            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.3");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");

            localBroadcast.CorrectFields();

            byte[] packetData = localBroadcast.ToArray();

            PublishPacket(captureDevice, packetData);

            callback.Verify(cb => cb(It.Is<IPacket>(p => p.ToArray().SequenceEqual(localBroadcast.ToArray()))), Times.Never);
        }

        [Fact]
        public void DoesNotPerformCallbackAfterDispose()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var callback = new Mock<Action<IPacket>>();

            var monitor = new NetworkAdapterMonitor(captureDevice.Object, localAddresses, callback.Object);


            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.2");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");

            localBroadcast.CorrectFields();

            byte[] packetData = localBroadcast.ToArray();

            monitor.Dispose();

            PublishPacket(captureDevice, packetData);

            callback.Verify(cb => cb(It.Is<IPacket>(p => p.ToArray().SequenceEqual(localBroadcast.ToArray()))), Times.Never);
        }

        private static void PublishPacket(Mock<ICaptureDevice> captureDevice, byte[] packetData)
        {
            captureDevice.Raise(
                d => d.OnPacketArrival += null,
                new CaptureEventArgs(
                    new RawCapture(PacketDotNet.LinkLayers.Ethernet, new PosixTimeval(), packetData),
                    captureDevice.Object));
        }
    }
}
