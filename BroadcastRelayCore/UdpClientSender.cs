using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutbreakLabs.BroadcastRelay.Core
{
    using System.Net;
    using System.Net.Sockets;

    using OutbreakLabs.LibPacketGremlin.Extensions;
    using OutbreakLabs.LibPacketGremlin.Packets;

    public sealed class UdpClientSender : IUdpSender, IDisposable
    {

        private readonly Socket socket;
        internal UdpClientSender()
        {

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
        }
        public void Send(IPv4<UDP> packet)
        {
            this.socket.SendTo(packet.ToArray(), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 500)); // This info is ignored, the headers are in the packet
        }

        public void Dispose()
        {
            this.socket.Dispose();
        }
    }
}
