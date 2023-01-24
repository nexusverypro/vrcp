#define WIN_DIST
#define DEBUG
#define IS_ANY_CPU

#define IS_APP

#if NET6_0
#define IS_NET6_0
#elif (NETCOREAPP3_1_OR_GREATER && !NET5_0_OR_GREATER)
#define IS_NETCAPP
#endif

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
using System.Text.Json.Serialization;

using VRCP.Core;
using VRCP.Core.Driver;
using VRCP.Core.HttpTraffic;
using VRCP.Core.Intro;
using VRCP.Core.Utils;
using VRCP.Network;
using VRCP.Log;
using VRCP.Application;
using System.Threading;

namespace VRCP
{
    public static class Program
    {
        private static VRCPNetAdapter[] GetAllNetworkAdapters()
        {
            VRCPNetAdapter[] networks = NetworkAPI.GetAdapters();
            return networks;
        }

        private static VRCPNetAdapter GetCurrentNetworkAdapter()
        {
            VRCPNetAdapter[] networks = GetAllNetworkAdapters();
            var activeAdapter = networks.First(x => x.NetType != NetworkInterfaceType.Loopback
                                && x.NetType != NetworkInterfaceType.Tunnel
                                && x.OperationalStatus == OperationalStatus.Up
                                && x.Name.StartsWith("vEthernet") == false);
            return activeAdapter;
        }

#if IS_APP
        public static void Main()
        {
            var appId = ApplicationHandler.GetGuid();
            Logger.Warning("Checking application session: " + appId);

            // check application instance
            ApplicationHandler.PreventMultipleInstance()
                .Then(() =>
                {
                    Logger.Trace("Application session verified");

                    Thread.Sleep(100);
                    Console.Clear();

                    IntroHelper.PlayIntro()
                        .Then(Program.AfterLogoResolve);
                })
                .Catch((ex) =>
                {
                    Logger.Error("Failed to check application session: A session of VRCP already exists.");
                });

            // in case the process lock fails
            Console.ReadKey();
            while (true) ;
        }

        private static void AfterLogoResolve()
        {
            Console.Clear();

            Cache.RescaleCacheCapacity(10);

            // store crucial os info
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
                var netAdapt = device.AdapterId;
                Cache.Add(((Guid)netAdapt).GetHashCode(), device);
                Logger<ProductionLoggerConfig>.LogDebug((i + 1) + ". " + device.Name + (string.IsNullOrEmpty(device.Description) ? ": (No description available)" : ": " + device.Description));
            }
            Logger<ProductionLoggerConfig>.LogDebug("=================================== networks ===================================")
                .Then(Console.WriteLine);
            Logger<ProductionLoggerConfig>.LogInformation("Initializing VRCP proxy drivers");
            try
            {

                var c = PacketPcapDriver.Create();
                c.Connect(currentNa.AdapterId)
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
            catch (DllNotFoundException dllEx)
            {
                string missingDll = dllEx.Message.Split('\'')[1].Split("\'")[0];
                Logger<ProductionLoggerConfig>.LogCritical($"Missing '{missingDll}.dll'. Cannot proceed with execution chain.");
                if (missingDll == "wpcap")
                {
                    Logger<ProductionLoggerConfig>.LogCritical($"Looks like you're missing the WPCAP dlls. To download them, go to https://www.winpcap.org/install.");
                }
                Logger<ProductionLoggerConfig>.LogCritical("Press any key to exit...");
                Console.ReadKey();
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
#endif
    }
}