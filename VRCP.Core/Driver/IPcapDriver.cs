// ---------------------------------- NOTICE ---------------------------------- //
// VRCP is made with the MIT License. Notices will be in their respective file. //
// ---------------------------------------------------------------------------- //

/*
MIT License

Copyright (c) 2023 Nexus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace VRCP.Core.Driver
{
    using PacketDotNet;
    using SharpPcap;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using VRCP.Network;

    /// <summary>
    /// Defines a WinPcap driver.
    /// </summary>
    public interface IPcapDriver : IDriver
    {
        /// <summary>
        /// Connects to the specified network.
        /// </summary>
        /// <param name="id">The selected NetworkAdapter id.</param>
        IPromise<DriverResult> Connect(NetworkAdapterId id);

        /// <summary>
        /// Forcefully sends a packet through a network.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        IPromise<DriverResult> SendPacket(Packet packet);

        /// <summary>
        /// Tells the driver to start receiving packet events.
        /// </summary>
        IPromise<DriverResult> BeginReceivePackets();

        /// <summary>
        /// Tells the driver to stop receiving packet events.
        /// </summary>
        IPromise<DriverResult> EndReceivePackets();

        /// <summary>
        /// Invokes when a packet has been captured my WinPcap.
        /// </summary>
        event Action<RawCapture, Packet> OnPacketCaptured;

        /// <summary>
        /// Specifies if we are receiving packets.
        /// </summary>
        bool IsReceivingPackets { get; }
    }
}
