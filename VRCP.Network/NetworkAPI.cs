using System;
using System.Collections.Generic;

using System.Net.NetworkInformation;

namespace VRCP.Network
{
    /// <summary>
    /// Specifies the low-level API for <see cref="NetworkInterface"/>.
    /// </summary>
    public static class NetworkAPI
    {
        private static NetImpl CurrentImpl = NetworkAPI.WinNet.LazyInitialize();

        /// <summary>
        /// Gets the current adapters being used.
        /// </summary>
        public static VRCPNetAdapter[] GetAdapters() => CurrentImpl.GetAdapters();

        /// <summary>
        /// Specifies whether a <see cref="VRCPNetAdapter"/> is being used.
        /// </summary>
        public static bool IsAdapterBeingUsed(VRCPNetAdapter adapter) => CurrentImpl.IsAdapterBeingUsed(adapter);

        // abstract class so i can make more impls for other operating systems
        private abstract class NetImpl
        {
            /// <summary>
            /// Gets the current adapters being used.
            /// </summary>
            public abstract VRCPNetAdapter[] GetAdapters();
            /// <summary>
            /// Specifies whether a <see cref="VRCPNetAdapter"/> is being used.
            /// </summary>
            public abstract bool IsAdapterBeingUsed(VRCPNetAdapter adapter);
        }

        // default impl for Windows
        private sealed class WinNet : NetImpl
        {
            private static Lazy<WinNet> _lazyInit = null;

            // lazy initializes a WinNet impl
            public static WinNet LazyInitialize()
            {
                if (_lazyInit == null) _lazyInit = new Lazy<WinNet>(Activator.CreateInstance<WinNet>());
                return _lazyInit.Value;
            }

            public override VRCPNetAdapter[] GetAdapters()
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                var vrcpAdapters = new VRCPNetAdapter[adapters.Length];

                for (int i = 0; i < adapters.Length; i++) vrcpAdapters[i] = VRCPNetAdapter.Construct(adapters[i]);
                return vrcpAdapters;
            }

            public override bool IsAdapterBeingUsed(VRCPNetAdapter adapter)
            {
                var adapters = this.GetAdapters();

                if (adapters.Any(x => x.AdapterId == adapter.AdapterId)) return adapter.IsBeingUsed();
                return false;
            }
        }
    }
}
