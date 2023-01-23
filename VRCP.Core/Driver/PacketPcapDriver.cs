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
    using System.ComponentModel;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Text;

    using VRCP.Core.Utils;
    using VRCP.Log;

    using VRCP.Network;

    [DriverOptions(DriverOptionsFlags.SINGLETON | DriverOptionsFlags.CACHE_ALLOWED)]
    public class PacketPcapDriver : BasePcapDriver
    {
        public override bool IsReceivingPackets => _isReceivingPackets;
        public override event Action<RawCapture, Packet> OnPacketCaptured;
        public event Action<bool> OnReceiveStateChanged;

        public override IPromise<DriverResult>  Connect(NetworkAdapterId id)
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            result.Then((res) =>
            {
                if (res.Result == DriverResult.OK_RESULT)
                {
                    var cacheItem = Cache.Get<NetworkInterface>(((Guid)id).GetHashCode());
                    Logger<ProductionLoggerConfig>.LogWarning("Listening on " + cacheItem.ToName());
                }
            });

            this.Internal_Connect(result, id);
            return result;
        }
        public override IPromise<DriverResult>  SendPacket(Packet packet)
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_SendPacket(result, packet);
            return result;
        }
        public override IPromise<DriverResult>  BeginReceivePackets()
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_ChangeReceiveState(result, true);
            return result;
        }
        public override IPromise<DriverResult>  EndReceivePackets()
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            this.Internal_ChangeReceiveState(result, false);
            return result;
        }

        /// <summary>
        /// Gets the current device ID.
        /// </summary>
        public          IPromise<string>        GetDeviceId()
        {
            Promise<string> result = new Promise<string>();
            this.Internal_GetDeviceId(result);
            return result;
        }
        /// <summary>
        /// Gets the current packet/device scheme
        /// </summary>
        public          IPromise<DeviceModes>   GetPacketScheme()
        {
            Promise<DeviceModes> result = new Promise<DeviceModes>();
            this.Internal_GetPacketScheme(result);
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
                c.Open(_deviceMode = DeviceModes.MaxResponsiveness, _readTimeout = 1000);
                c.OnPacketArrival += _onPacketArrival = Internal_OnPacketArrival;
                c.Filter = _filter = "host api.vrchat.cloud and tcp";
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

                //Internal_GetConnectHandshake(packet);

                // gather responses
                var rawResponse = transportPacket.Bytes;
                var decodedResponse = Encoding.UTF8.GetString(rawResponse);

                // log if there is data
                if (rawResponse.Length > 0 && (transportPacket.DestinationPort == 443
                                            || transportPacket.DestinationPort == 80))
                {
                    // this could only show up if we are using HTTP or from a CONNECT packet
                    bool fromVRChatServers = decodedResponse.Contains("api.vrchat.cloud");

                    // initial CONNECT packet identification
                    if (fromVRChatServers)
                    {
                        var bytes = rawResponse.ToList().ToHexCodes();
                        Logger.Warning("{0}, {1}", bytes[0], bytes[1]);
                        Logger<ProductionLoggerConfig>.LogInformation($"0x{rawResponse[0].ToString("x").ToUpper()}, 0x{rawResponse[1].ToString("x").ToUpper()}: {(fromVRChatServers ? "Verified packet: " : "Unknown packet: ")}Received TCP packet '{transportPacket.Checksum.ToString("x").PadLeft(4, 'f')}' with length of {rawResponse.Length} bytes");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Error on Packet arrival: " + ex.Message);
            }
        }
        private void Internal_GetDeviceId(Promise<string> p)
        {
            p.Resolve($"0x{(((_currentDevice.GetHashCode() * 100) >> (int)_deviceMode) * 918).ToString("x")}");
        }
        private void Internal_GetPacketScheme(Promise<DeviceModes> p)
        {
            p.Resolve(_deviceMode);
        }

        private void Internal_GetConnectHandshake(Packet packet)
        {
            var tcp = packet.Extract<TcpPacket>();
            if (tcp == null) return;

            var rawResponse = tcp.Bytes;
            if (rawResponse.Length > 50 && tcp.PayloadData[0] == 0x05 || tcp.PayloadData[0] == 0xD7)
            {
                // extract the destination host and port
                var destHost = Encoding.ASCII.GetString(tcp.PayloadData, 3, tcp.PayloadData[2]);
                var destPort = (tcp.PayloadData[tcp.PayloadData[2] + 3] << 8) + tcp.PayloadData[tcp.PayloadData[2] + 4];

                // extract the protocol being used
                var protocol = Encoding.ASCII.GetString(tcp.PayloadData, tcp.PayloadData[2] + 5, tcp.PayloadData[tcp.PayloadData[2] + 4]);

                Console.WriteLine("CONNECT packet captured for session: {0}:{1} using {2}", destHost, destPort, protocol);
            }
        }

        private int _readTimeout;
        private DeviceModes _deviceMode;

        private ILiveDevice _currentDevice;

        private PacketArrivalEventHandler _onPacketArrival;

        private string _filter;

        private bool _isReceivingPackets;
        private BackgroundWorker _backgroundWorker;
    }
}
