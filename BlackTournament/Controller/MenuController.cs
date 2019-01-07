using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Properties;

namespace BlackTournament.Controller
{
    public class MenuController : ControllerBase
    {
        private String _Message;
        private MainMenu _State;
        private BlackTournamentClient _LanDiscoveryClient;

        public MenuController(Game game) : base(game)
        {
        }

        public void Activate(String message = null)
        {
            _Message = message;
            Activate(_State = new MainMenu(_Game.Core));
        }

        protected override void StateLoadingFailed()
        {
            throw new Exception("FUBAR");
        }

        protected override void StateReady()
        {
            _LanDiscoveryClient = new BlackTournamentClient(Settings.Default.PlayerName); // TODO : move discovery into a seperate client class - there WAN servers (sh)could be handled as well
            _Game.Core.OnUpdate += _LanDiscoveryClient.Update;
            _LanDiscoveryClient.LanServerListUpdated += HandleLanServerListUpdated;
            _LanDiscoveryClient.DiscoverLanServers(Net.Net.DEFAULT_PORT); // consider making the entire discovery on demand - moving it into browse clicked
            HandleLanServerListUpdated(); // HACK

            if (!String.IsNullOrWhiteSpace(_Message)) _State.DisplayPopupMessage(_Message);
            _State.Browse += State_BrowseClicked;
            _State.Host += State_HostClicked;
        }

        private void HandleLanServerListUpdated()
        {
            var l = _LanDiscoveryClient.LanServers.Concat(new(IPEndPoint, Net.Data.ServerInfo)[]
            {
                (new IPEndPoint(123456789, 4711), new Net.Data.ServerInfo("TestServer", "TestMap"){ Ping = 32 }),
                (new IPEndPoint(456456456, 4711), new Net.Data.ServerInfo("TestServer2", "OtherMap"){ Ping = 17 })
            }).ToArray();
            Log.Debug("Refreshing server list with", l.Length, "Servers");
            _State.UpdateServerList(l);
        }

        private void State_BrowseClicked()
        {
            _LanDiscoveryClient.DiscoverLanServers(Net.Net.DEFAULT_PORT, false);
        }

        private void State_HostClicked()
        {
            var name = String.IsNullOrWhiteSpace(_State.Servername) ? $"{Settings.Default.PlayerName}'s Server" : _State.Servername;
            var port = _State.Port < 1 ? Net.Net.DEFAULT_PORT : _State.Port;

            if (_Game.Host(port, _State.Mapname, name)) _Game.Connect(Net.Net.DEFAULT_HOST, port);
            else _State.DisplayPopupMessage($"Failed to host {_State.Mapname} on {Net.Net.DEFAULT_HOST}:{port}. Is the map and port valid?");
        }

        protected override void StateReleased()
        {
            _Game.Core.OnUpdate -= _LanDiscoveryClient.Update;
            _LanDiscoveryClient.LanServerListUpdated -= HandleLanServerListUpdated;
            _LanDiscoveryClient.Dispose();
            _LanDiscoveryClient = null;

            _State.Browse -= State_BrowseClicked;
            _State.Host -= State_HostClicked;
            _State = null;
        }
    }
}