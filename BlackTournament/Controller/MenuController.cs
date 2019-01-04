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
        private BlackTournamentClient _Client;

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
            _Client = new BlackTournamentClient(Settings.Default.PlayerName);
            _Client.LanServerListUpdated += HandleLanServerListUpdated;
            _Client.DiscoverLanServers(Net.Net.DEFAULT_PORT); // move

            if (!String.IsNullOrWhiteSpace(_Message)) _State.DisplayPopupMessage(_Message);
            _State.Browse += State_BrowseClicked;
            _State.Host += State_HostClicked;
            // TODO: restore UI State
        }

        private void HandleLanServerListUpdated()
        {
            var l = _Client.LanServers.Concat(new(IPEndPoint, string)[] { (new IPEndPoint(123456789, 4711), "Testserver") }).ToArray();
            _State.UpdateServerList(l);
        }

        private void State_BrowseClicked()
        {
            _State.OpenServerBrowser(true); 
        }

        private void State_HostClicked()
        {
            var name = String.IsNullOrWhiteSpace(_State.Servername) ? $"{Settings.Default.PlayerName}'s Server" : _State.Servername;
            var port = _State.Port < 1 ? Net.Net.DEFAULT_PORT : _State.Port;

            if (_Game.Host(_State.Mapname, name, port)) _Game.Connect(Net.Net.DEFAULT_HOST, port);
            else _State.DisplayPopupMessage($"Failed to host {_State.Mapname} on {Net.Net.DEFAULT_HOST}:{port}. Is the map and port valid?");
        }

        protected override void StateReleased()
        {
            _Client.LanServerListUpdated -= HandleLanServerListUpdated;
            _Client.Dispose();
            _Client = null;

            _State.Browse -= State_BrowseClicked;
            _State.Host -= State_HostClicked;
            _State = null;
        }
    }
}