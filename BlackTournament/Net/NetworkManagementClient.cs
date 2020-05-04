using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using BlackCoat.Network;
using BlackTournament.Net.Data;
using System.Net;
using System.Diagnostics;
using BlackTournament.Properties;

namespace BlackTournament.Net
{
    public class NetworkManagementClient : Client<ManagementMessage> // TODO : Move to library
    {
        private Dictionary<IPEndPoint, ServerInfo> _LanServers;
        private List<ServerInfo> _WanServers;
        private Stopwatch _DiscoveryTimer;


        public IEnumerable<ServerInfo> LanServers => _LanServers.Values;
        public IEnumerable<ServerInfo> WanServers => _WanServers;


        // Events
        public event Action OnConnect = () => { };
        public event Action OnDisconnect = () => { };
        public event Action LanServersUpdated = () => { };
        public event Action WanServersUpdated = () => { };



        public NetworkManagementClient():base(Game.ID)
        {
            _LanServers = new Dictionary<IPEndPoint, ServerInfo>();
            _WanServers = new List<ServerInfo>();
            _DiscoveryTimer = new Stopwatch();
        }


        // CONTROL
        public void Connect()
        {
            Connect(Settings.Default.ManagementServerAdress, Net.DEFAULT_PORT, String.Empty);
        }

        protected override void Connected()
        {
            OnConnect.Invoke();
        }

        protected override void Disconnected()
        {
            OnDisconnect.Invoke();
        }


        // OUTGOING
        public void DiscoverLanServers(int port, bool clearOldEntries = true)
        {
            if (clearOldEntries) _LanServers.Clear();
            _BasePeer.Start();
            _DiscoveryTimer.Reset();
            _DiscoveryTimer.Start();
            _BasePeer.DiscoverLocalPeers(port);
        }

        public void DiscoverWanServers()
        {
            Send(ManagementMessage.RequestPublicServers);
        }

        public void MakePublic(ServerInfo info)
        {
            Send(ManagementMessage.AnnounceServer, m => info.Serialize(m, true));
        }


        // INCOMMING
        protected override void DiscoveryResponseReceived(NetIncomingMessage msg)
        {
            var latency = (int)(msg.SenderConnection?.AverageRoundtripTime * 1000);
            if(latency == 0) latency = (int)_DiscoveryTimer.ElapsedMilliseconds;

            var endPoint = msg.SenderEndPoint;
            if (_LanServers.ContainsKey(endPoint))
            {
                var server = _LanServers[endPoint];
                server.Deserialize(msg);
                server.Ping = latency;
            }
            else
            {
                msg.ReadInt32(); // Skip serverInfo.id
                var server = new ServerInfo(endPoint, msg) { Ping = latency };
                _LanServers[endPoint] = server;
            }
            LanServersUpdated.Invoke();
        }

        protected override void ProcessIncommingData(ManagementMessage message, NetIncomingMessage msg)
        {
            switch (message)
            {
                case ManagementMessage.AnnounceServer:
                case ManagementMessage.RequestPublicServers:
                    // Server Only
                    break;
                case ManagementMessage.UpdatePublicServers:
                    UpdatePublicServers(msg);
                    break;
            }
        }

        private void UpdatePublicServers(NetIncomingMessage msg)
        {
            _WanServers.Clear();
            var servers = msg.ReadInt32();
            for (int i = 0; i < servers; i++)
            {
                var endpoint = new IPEndPoint(new IPAddress(msg.ReadInt64()), msg.ReadInt32());
                msg.ReadInt32(); // Skip serverInfo.id
                _WanServers.Add(new ServerInfo(endpoint, msg));
            }
            WanServersUpdated.Invoke();
        }
    }
}