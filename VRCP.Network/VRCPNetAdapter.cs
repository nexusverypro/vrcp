using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace VRCP.Network
{
    /// <summary>
    /// Specifies a <see cref="NetworkInterface"/> in a controlled manner.
    /// </summary>
    public class VRCPNetAdapter
    {
        private VRCPNetAdapter(NetworkInterface ni)
        {
            this._interface = ni;
            this._name = this._interface.Name;
            this._description = this._interface.Description;
            this._adapterId = this._interface.Id;
            this._opStatus = this._interface.OperationalStatus;
            this._netType = this._interface.NetworkInterfaceType;
        }

        /// <summary>
        /// Constructs a new <see cref="VRCPNetAdapter"/> from a <see cref="NetworkInterface"/>.
        /// </summary>
        internal static VRCPNetAdapter Construct(NetworkInterface netInterface) => new VRCPNetAdapter(netInterface);

        public bool IsBeingUsed() => this._opStatus == OperationalStatus.Up;

        #region Statistics
        public VRCP_IPStatistics GetCurrentIPStatistics()
        {
            return new VRCP_IPStatistics(_interface.GetIPStatistics());
        }
        #endregion

        #region Publics
        /// <summary>
        /// The name of the adapter.
        /// </summary>
        public string Name => _name;
        /// <summary>
        /// The description of the adapter.
        /// </summary>
        public string Description => _description;
        /// <summary>
        /// The ID of the adapter.
        /// </summary>
        public NetworkAdapterId AdapterId => _adapterId;
        /// <summary>
        /// The operation status of the adapter.
        /// </summary>
        public OperationalStatus OperationalStatus => _opStatus;
        /// <summary>
        /// The net type of the adapter.
        /// </summary>
        public NetworkInterfaceType NetType => _netType;
        #endregion
        #region Privates
        private string _name;
        private string _description;
        private NetworkAdapterId _adapterId;
        private OperationalStatus _opStatus;
        private NetworkInterfaceType _netType;

        private NetworkInterface _interface;
        #endregion
    }

    /// <summary>
    /// Specifies statistics for a <see cref="NetworkInterface"/>.
    /// </summary>
    public class VRCP_IPStatistics
    {
        internal VRCP_IPStatistics(IPInterfaceStatistics statistics) => _stats = statistics;

        public double SentInMegabytes => _stats.BytesSent / 0.000001;
        public double ReceivedInMegabytes => _stats.BytesReceived / 0.000001;

        public int IncomingPacketsDropped => (int)_stats.IncomingPacketsDiscarded;
        public int OutgoingPacketsDropped => (int)_stats.OutgoingPacketsDiscarded;

        private IPInterfaceStatistics _stats;
    }
}
