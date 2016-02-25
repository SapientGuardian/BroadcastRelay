// -----------------------------------------------------------------------
//  <copyright file="ARPHelper.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;

    public class ARPHelper
    {
        //http://snipplr.com/view/54687/

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

        /// <summary>
        ///     Gets the MAC address (<see cref="PhysicalAddress" />) associated with the specified IP.
        /// </summary>
        /// <param name="ipAddress">The remote IP address.</param>
        /// <returns>The remote machine's MAC address.</returns>
        public static PhysicalAddress GetMacAddress(IPAddress ipAddress)
        {
            const int MacAddressLength = 6;

            var length = MacAddressLength;

            var macBytes = new byte[MacAddressLength];

            SendARP(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macBytes, ref length);

            return new PhysicalAddress(macBytes);
        }
    }
}