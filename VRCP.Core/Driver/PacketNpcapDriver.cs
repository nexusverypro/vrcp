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
    using Libpcap;
    using OpenSSL.PrivateKeyDecoder;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using VRCP.Core.Utils;
    using VRCP.Log;
    using VRCP.Network;

    [DriverOptions(DriverOptionsFlags.SINGLETON | DriverOptionsFlags.CACHE_ALLOWED)]
    public class PacketNpcapDriver : BaseNpcapDriver
    {
        public override bool IsReceivingPackets => _isReceivingPackets;
        /// <summary>
        /// Invokes when a packet has been captured my WinPcap.
        /// </summary>
        public override event PacketCallback OnPacketCaptured;
        public event Action<bool> OnReceiveStateChanged;

        public override IPromise<DriverResult> Connect(NetworkAdapterId id)
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
            var devices = Pcap.ListDevices();

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
        private void Internal_StartConnect(Promise<DriverResult> p, PcapDevice device)
        {
            if (device == null)
            {
                p.Reject("'device' is null.");
                return;
            }

            try
            {
                var cd = _currentDevice = device;
                var cpd = _currentOpenedDevice = Pcap.OpenDevice(cd);
                var cpdr = cpd.Activate();
                cpd.Filter = "tcp";
                if (cpdr == PcapActivateResult.Success) cpd.Dispatch(9000, Internal_OnPacketReceived);

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
        private void Internal_OnPacketReceived(Pcap pcap, ref Packet packet)
        {
            try
            {
                Logger.Information(Encoding.UTF8.GetString(packet.Data));
            }
            catch (Exception ex)
            {
                Logger.Critical("Error on Packet arrival: " + ex.ToString());
            }
        }

        private PcapDevice _currentDevice;
        private DevicePcap _currentOpenedDevice;

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
