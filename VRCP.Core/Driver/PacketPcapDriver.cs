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

using Microsoft.VisualBasic;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VRCP.Core.Utils;
using VRCP.Log;

namespace VRCP.Core.Driver
{
    [ComVisible(true)]
    [DriverOptions(DriverOptionsFlags.SINGLETON | DriverOptionsFlags.CACHE_ALLOWED)]
    public class PacketPcapDriver : BasePcapDriver
    {
        public override bool IsReceivingPackets => _isReceivingPackets;
        public override event Action<RawCapture, Packet> OnPacketCaptured;
        public event Action<bool> OnReceiveStateChanged;

        public override IPromise<DriverResult> Connect(NetworkAdapterId id)
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_Connect(result, id);
            return result;
        }
        public override IPromise<DriverResult> SendPacket(Packet packet)
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_SendPacket(result, packet);
            return result;
        }
        public override IPromise<DriverResult> BeginReceivePackets()
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_ChangeReceiveState(result, true);
            return result;
        }
        public override IPromise<DriverResult> EndReceivePackets()
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_ChangeReceiveState(result, false);
            return result;
        }

        private void Internal_Connect(Promise<DriverResult> p, NetworkAdapterId id)
        {
            var devices = CaptureDeviceList.Instance;

            // If no device exists, print an error
            if (devices.Count < 1)
            {
                p.Reject("No viable device found on this machine");
                return;
            }

            string networkId = "\\Device\\NPF_{" + id + "}";
            var current = devices.First((x) => x.Name == networkId);
            if (current == null)
            {
                p.Reject("No devices with the ID " + id);
                return;
            }
            this.Internal_StartConnect(p, current);
        }

        private void Internal_StartConnect(Promise<DriverResult> p, ILiveDevice device)
        {
            if (device == null)
            {
                p.Reject("'device' is null.");
                return;
            }

            try
            {
                var c = _currentDevice = device;
                c.Open(DeviceModes.Promiscuous, 1000);
                c.OnPacketArrival += Internal_OnPacketArrival;
                c.Filter = "host api.vrchat.cloud and tcp";
                c.StartCapture();

                p.Resolve(DriverResult.CreateFrom(DriverResult.OK_RESULT));
            }
            catch (Exception ex)
            {
                p.Reject(ex);
            }
        }

        private void Internal_SendPacket(Promise<DriverResult> p, Packet packet)
        {
            try
            {
                _currentDevice.SendPacket(packet);

                p.Resolve(DriverResult.CreateFrom(DriverResult.OK_RESULT));
            }
            catch (Exception ex)
            {
                p.Reject(ex);
            }
        }

        private void Internal_ChangeReceiveState(Promise<DriverResult> p, bool startReceiving)
        {
            try
            {
                _isReceivingPackets = startReceiving;
                this.OnReceiveStateChanged.SafeInvoke(startReceiving);

                p.Resolve(DriverResult.CreateFrom(DriverResult.OK_RESULT));
            }
            catch (Exception ex)
            {
                p.Reject(ex);
            }
        }

        private void Internal_OnPacketArrival(object? sender, PacketCapture e)
        {
            try
            {
                var rawCapture = default(RawCapture);
                var packet = default(Packet);

                this.OnPacketCaptured.SafeInvoke(rawCapture = e.GetPacket(), 
                                                 packet     = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data));

                var transportPacket = packet.Extract<TcpPacket>();
                if (transportPacket == null) return;

                // gather responses
                var rawResponse = transportPacket.Bytes;
                var decodedResponse = Encoding.UTF8.GetString(rawResponse);

                // log if there is data
                if (rawResponse.Length > 0 && (transportPacket.DestinationPort == 443
                                            || transportPacket.DestinationPort == 80))
                {
                    // this could only show up if we are using HTTP
                    bool fromVRChatServers = decodedResponse.Contains("api.vrchat.cloud");

                    // initial CONNECT packet identification
                    if (fromVRChatServers)
                    {
                        Logger<ProductionLoggerConfig>.LogInformation($"0x{rawResponse[0].ToString("x").ToUpper()}, 0x{rawResponse[1].ToString("x").ToUpper()}: {(fromVRChatServers ? "Verified packet: " : "Unknown packet: ")}Received TCP packet '{transportPacket.Checksum.ToString("x").PadLeft(4, 'f')}' with length of {rawResponse.Length} bytes");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Error on Packet arrival: " + ex.Message);
            }
        }

        private ILiveDevice _currentDevice;
        private bool _isReceivingPackets;
        private BackgroundWorker _backgroundWorker;
    }
}
