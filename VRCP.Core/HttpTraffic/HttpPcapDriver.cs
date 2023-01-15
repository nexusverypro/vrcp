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

namespace VRCP.Core.HttpTraffic
{
    using SharpPcap;
    using PacketDotNet;
    using PacketDotNet.Ieee80211;

    using Whois;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;

    using System.Threading.Tasks;

    using VRCP.Log;

    using System.Text.Json;
    using System.Text.RegularExpressions;

    using System.Net.Sockets;
    using Tokens.Extensions;

    public class HttpPcapDriver
    {
        private static ILiveDevice _currentDevice;

        public static IPromise Initialize(string filterOp, string id)
        {
            Promise promise = new Promise();
            HttpPcapDriver.Internal_Initialize(promise, filterOp, id);
            return promise;
        }

        private static void Internal_Initialize(Promise p, string filterOp, string id)
        {
            try
            {
                string networkId = "\\Device\\NPF_" + id;
                Logger<ProductionLoggerConfig>.LogWarning($"Retrieving device of '{networkId}'");
                // Retrieve the current device list
                var devices = CaptureDeviceList.Instance;

                // If no device exists, print an error
                if (devices.Count < 1)
                {
                    Logger<ProductionLoggerConfig>.LogWarning("No viable device found on this machine");
                    return;
                }

                var current = devices.First((x) => x.Name == networkId);

                // If there is no NA, throw an error because we wont be able to read packets
                _currentDevice = current == null ? throw new BasicPcapException("Couldnt identify current Network Adapter.") : current; // Set the current device
                _currentDevice.Open(DeviceModes.Promiscuous, 1000); // Open the device
                _currentDevice.Filter = filterOp; // Set the filter
                _currentDevice.OnCaptureStopped += Device_OnCaptureStopped;
                _currentDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival); // Capture and process packets
                _currentDevice.StartCapture(); // Start the capture

                Logger<ProductionLoggerConfig>.LogInformation("Listening on " + id.Replace("{", "")
                                                                                  .Replace("}", ""));
                p.Resolve();
            }
            catch (Exception ex)
            {
                p.Reject(ex);
            }
        }

        public static void Close()
        {
            // Stop the capturing process
            _currentDevice.StopCapture();

            // Close the current pcap device
            _currentDevice.Close();
        }

        private static void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            // Extract the IP address
            var ipPacket = packet.Extract<IPPacket>();
            // Extract the transport layer packet
            var transportPacket = packet.Extract<TcpPacket>();
            //var response = LookupClient.Lookup(ipPacket.SourceAddress.ToString());
            //Logger<ProductionLoggerConfig>.LogInformation(response.DomainName.Value);
            bool isResponse = transportPacket != null;
            string response = "";
            if (isResponse && transportPacket.DestinationPort == 443 || transportPacket.DestinationPort == 80)
            {
                Logger<ProductionLoggerConfig>.LogTrace("Received response packet");
                try
                {
                    // Parse the payload data as an HTTP response
                    response = Encoding.UTF8.GetString(transportPacket.PayloadData);
                    Logger<ProductionLoggerConfig>.LogInformation(response);
                }
                catch (Exception ex)
                {
                    Logger<ProductionLoggerConfig>.LogCritical($"Failed to read packet: 0x{ex.HResult.ToString("x").PadLeft(8, '0')}");
                }
            }

        }

        private static void Device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
        {
            Logger<ProductionLoggerConfig>.LogWarning("Capturing stopped");
        }

        public static WhoisLookup LookupClient => Cache.GetOrAdd<WhoisLookup>(0x0A817FE1, new WhoisLookup());
    }

    public class BasicPcapException : Exception
    {
        public BasicPcapException(string msg) : base(msg) { }
    }
}
