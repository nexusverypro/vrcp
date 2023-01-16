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

using System;
using System.Net.NetworkInformation;
using VRCP.Core;
using VRCP.Core.Driver;
using VRCP.Core.HttpTraffic;
using VRCP.Log;

namespace VRCP
{
    public static class Program
    {
        private static NetworkInterface[] GetAllNetworkAdapters()
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
            return networks;
        }

        private static NetworkInterface GetCurrentNetworkAdapter()
        {
            NetworkInterface[] networks = GetAllNetworkAdapters();
            var activeAdapter = networks.First(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                && x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                && x.OperationalStatus == OperationalStatus.Up
                                && x.Name.StartsWith("vEthernet") == false);
            return activeAdapter;
        }

        public static void Main()
        {
            Cache.RescaleCacheCapacity(10);
            Cache.Add(0x0DE1AF2, Environment.OSVersion.VersionString);
            Cache.Add(0x0DE1AF3, Environment.OSVersion.ServicePack);
            Cache.Add(0x0DE1AF4, Environment.OSVersion.Version);
            Cache.Add(0x0DE1AF5, Environment.ProcessId);
            Cache.Add(0x0DE1AF6, Environment.UserDomainName);
            Cache.Add(0x0DE1AF7, Environment.UserName);
            Cache.Add(0x0DE1AF8, Environment.MachineName);

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            var networkAdapters = GetAllNetworkAdapters();
            var currentNa = GetCurrentNetworkAdapter();

            Logger<ProductionLoggerConfig>.LogDebug("=================================== networks ===================================");
            for (int i = 0; i != networkAdapters.Count(); ++i)
            {
                var device = networkAdapters[i];
                Logger<ProductionLoggerConfig>.LogDebug((i + 1) + ". " + device.Name + (string.IsNullOrEmpty(device.Description) ? ": (No description available)" : ": " + device.Description));
            }
            Logger<ProductionLoggerConfig>.LogDebug("=================================== networks ===================================")
                .Then(Console.WriteLine);
            Logger<ProductionLoggerConfig>.LogInformation("Initializing VRCP proxy drivers");
            try
            {
                var c = (PacketPcapDriver)IDriver.Create<PacketPcapDriver>();
                c.Connect(currentNa.Id)
                    .Then(res =>
                    {
                        c.BeginReceivePackets()
                            .Then(rex =>
                            {
                                Logger<ProductionLoggerConfig>.LogInformation("Successfully initialized HttpPcapDriver");
                            })
                            .Catch(ex => Logger<ProductionLoggerConfig>.LogCritical("Failed to initialize driver.\n\tStack message - " + ex.Message + "; at " + ex.TargetSite.Name));
                    })
                    .Catch(ex => Logger<ProductionLoggerConfig>.LogCritical("Failed to initialize driver.\n\tStack message - " + ex.Message + "; at " + ex.TargetSite.Name));

            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical(ex.ToString());
                Logger<ProductionLoggerConfig>.LogCritical("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
        }
    }
}