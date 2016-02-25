using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutbreakLabs.BroadcastRelay.Core
{
    using OutbreakLabs.LibPacketGremlin.Packets;

    /// <summary>
    /// Interface for sending UDP packets
    /// </summary>
    public interface IUdpSender
    {
        /// <summary>
        /// Sends the specified IPv4+UDP payload.
        /// </summary>
        /// <param name="packet">The payload.</param>        
        void Send(IPv4<UDP> packet);
    }
}
