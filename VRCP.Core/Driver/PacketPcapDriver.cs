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
    using OpenSSL.PrivateKeyDecoder;
    using PacketDotNet;
    using SharpPcap;

    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using VRCP.Core.Utils;
    using VRCP.Log;

    using VRCP.Network;

    [DriverOptions(DriverOptionsFlags.SINGLETON | DriverOptionsFlags.CACHE_ALLOWED)]
    public class PacketPcapDriver : BasePcapDriver
    {
        public override bool IsReceivingPackets => _isReceivingPackets;
        /// <summary>
        /// Invokes when a packet has been captured my WinPcap.
        /// </summary>
        public override event Action<RawCapture, Packet> OnPacketCaptured;
        public event Action<bool> OnReceiveStateChanged;

        public override IPromise<DriverResult>  Connect(NetworkAdapterId id)
        {
            Promise<DriverResult> result = new Promise<DriverResult>();
            result.Then((res) =>
            {
                if (res.Result == DriverResult.OK_RESULT)
                {
                    var cacheItem = Cache.Get<VRCPNetAdapter>(((Guid)id).GetHashCode());

                    _netAdapter = cacheItem;
                    Logger<ProductionLoggerConfig>.LogWarning("Listening on " + cacheItem.ToName());
                }
            });
            this.Internal_Connect(result, id);
            return result;
        }
        /// <summary>
        /// Forcefully sends a packet through a network.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
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

                var ipPacket = packet.Extract<IPPacket>();
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
                    // check if we have certificate data
                    if (Encoding.UTF8.GetString(transportPacket.Bytes).Contains("cert"))
                    {
                        Logger.Trace("Certificate data found");
                        _sessionExtractor.CertificateData = rawResponse;
                    }

                    // initial CONNECT packet identification
                    var cCode = _sessionExtractor.CurrentCode();

                    // this could only show up if we are using HTTP or from a CONNECT packet
                    // rolling codes would only work with HTTPS
                    bool fromVRChatServers = decodedResponse.Contains("api.vrchat.cloud") || _sessionExtractor.IsRollingCode(rawResponse);

                    // store another code
                    if (!fromVRChatServers && !_sessionExtractor.IsRollingCode(rawResponse))
                        _sessionExtractor.StoreConnectByte(rawResponse[0]);

                    // rolling codes checker
                    if (fromVRChatServers)
                    {
                        var bytes = rawResponse.ToList().ToHexCodes();
                        _sessionExtractor.StoreConnectByte(rawResponse[0]);
                        if (_sessionExtractor.HasCertData)
                        {
                            var data = _sessionExtractor.DecryptSession(_sessionExtractor.CertificateData, rawResponse);
                        }

                        var cert = _sessionExtractor.GetCertificate();

                        Logger.Information("CRT{2}: Received incoming packet '{0}' from {1}", transportPacket.Checksum.ToString("x").PadLeft(4, 'f'), ipPacket.DestinationAddress.MapToIPv4().ToString(), cert.GetSerialNumberString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Error on Packet arrival: " + ex.ToString());
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

        [Obsolete("Not used, everything is handled in Internal_OnPacketArrival()", true)]
        private void Internal_GetConnectHandshake(Packet packet)
        {
            var tcp = packet.Extract<TcpPacket>();
            if (tcp == null) return;

            var rawResponse = tcp.Bytes;

            // bytes can change at anytime, but they seem to follow a specific pattern:
            //
            // 0xDD will but turned into 0xDE after the TLS session expires, basically
            // shifting by 1 byte, but yet 0xDD can be any byte
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
        private TLSSessionExtractor _sessionExtractor = new TLSSessionExtractor();
        private VRCPNetAdapter _netAdapter;

        private class TLSSessionExtractor
        {
            public TLSSessionExtractor() { }

            public void StoreConnectByte(int b)
            {
                _b = b;
            }

            public int CurrentCode() => _b;

            public bool IsRollingCode(byte[] payload) => _b == payload[0] || _b == payload[0] + 1;

            public byte[] DecryptSession(byte[] sslCert, byte[] payload)
            {
                // check rolling code availibility
                if (!IsRollingCode(payload)) return null;

                IOpenSSLPrivateKeyDecoder decoder = new OpenSSLPrivateKeyDecoder();

                var cryptoServiceProvider = decoder.Decode(Encoding.UTF8.GetString(sslCert));

                var data = cryptoServiceProvider.Decrypt(payload, false);
                return data;
            }

            public X509Certificate GetCertificate() => X509Certificate2.CreateFromCertFile(Environment.CurrentDirectory + CERT_PATH);

            public bool HasCertData => CertificateData != null;
            public byte[] CertificateData { get; internal set; } = null;
            private int _b;

            private const string CERT_PATH = "\\common\\vrchat_cloud.crt";
        }
    }
}
