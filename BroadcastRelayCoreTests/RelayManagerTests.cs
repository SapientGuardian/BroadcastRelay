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

    using FluentAssertions;

    using OutbreakLabs.LibPacketGremlin.Packets;

    public class RelayManagerTests
    {
        [Fact]
        public void RelaysAndRewritesLocalBroadcast()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };            

            var sendDevice = new Mock<IUdpSender>();

            var relayManager = new RelayManager(localAddresses, sendDevice.Object);


            relayManager.AddDestination(IPAddress.Parse("192.168.3.5"));

            relayManager.EnableCaptureDevice(captureDevice.Object);

            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));            

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.2");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");
            localBroadcast.Payload.Payload.SourcePort = 54321;
            localBroadcast.Payload.Payload.DestPort = 12345;

            localBroadcast.CorrectFields();
            
            PublishPacket(captureDevice, localBroadcast.ToArray());

            sendDevice.Verify(cb => cb.Send(It.Is<IPv4<UDP>>(p=>p.DestAddress.Equals(IPv4Address.Parse("192.168.3.5")))), Times.Once);
        }

        [Fact]
        public void CanDisableCaptureDevice()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var sendDevice = new Mock<IUdpSender>();

            var relayManager = new RelayManager(localAddresses, sendDevice.Object);


            relayManager.AddDestination(IPAddress.Parse("192.168.3.5"));

            relayManager.EnableCaptureDevice(captureDevice.Object);

            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.2");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");

            localBroadcast.CorrectFields();

            relayManager.DisableCaptureDevice(captureDevice.Object);

            PublishPacket(captureDevice, localBroadcast.ToArray());

            sendDevice.Verify(cb => cb.Send(It.IsAny<IPv4<UDP>>()), Times.Never);
        }

        [Fact]
        public void CanRemoveDestination()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var sendDevice = new Mock<IUdpSender>();

            var relayManager = new RelayManager(localAddresses, sendDevice.Object);


            relayManager.AddDestination(IPAddress.Parse("192.168.3.5"));
            

            relayManager.EnableCaptureDevice(captureDevice.Object);

            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.2");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");

            localBroadcast.CorrectFields();

            relayManager.DisableCaptureDevice(captureDevice.Object);

            relayManager.RemoveDestination(IPAddress.Parse("192.168.3.5"));

            PublishPacket(captureDevice, localBroadcast.ToArray());

            sendDevice.Verify(cb => cb.Send(It.IsAny<IPv4<UDP>>()), Times.Never);
        }

        [Fact]
        public void IncrementsPacketsRelayed()
        {
            var captureDevice = new Mock<ICaptureDevice>();

            var localAddresses = new[] { IPAddress.Parse("192.168.1.2") };

            var sendDevice = new Mock<IUdpSender>();

            var relayManager = new RelayManager(localAddresses, sendDevice.Object);


            relayManager.AddDestination(IPAddress.Parse("192.168.3.5"));

            relayManager.EnableCaptureDevice(captureDevice.Object);

            var localBroadcast = EthernetIIFactory.Instance.Default(IPv4Factory.Instance.Default(UDPFactory.Instance.Default(GenericFactory.Instance.Default())));

            localBroadcast.Payload.SourceAddress = IPv4Address.Parse("192.168.1.2");
            localBroadcast.Payload.DestAddress = IPv4Address.Parse("192.168.1.255");

            localBroadcast.CorrectFields();

            relayManager.PacketsRelayed.Should().Be(0);

            PublishPacket(captureDevice, localBroadcast.ToArray());

            relayManager.PacketsRelayed.Should().Be(1);
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
