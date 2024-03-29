﻿// ---------------------------------- NOTICE ---------------------------------- //
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

using PacketDotNet;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace VRCP.Core.HttpTraffic
{
    public static class ConnectHandshakeData
    {
        public static SslHandshake GetHandshake(Packet packet)
        {
            using (var stream = new MemoryStream(packet.PayloadData))
            using (var reader = new BinaryReader(stream))
            {
                // Check if the packet is a TCP packet
                var tcpPacket = packet.Extract<TcpPacket>();
                if (tcpPacket != null && 
                    tcpPacket.PayloadData[0] == 0x16 && 
                    tcpPacket.PayloadData[1] == 0x03)
                {

                    // Check if the packet contains SSL/TLS data

                    // Extract the SSL/TLS handshake data from the packet
                    var sslHandshake = new SslHandshake(tcpPacket.PayloadData);
                    return sslHandshake;
                }
            }
            return null;
        }
    }

    public class SslHandshake
    {
        public SslHandshake(byte[] data)
        {
            this._data = data;
            this.ExtractInfo();
        }

        private void ExtractInfo()
        {
        }

        private byte[] _data;
    }
}
