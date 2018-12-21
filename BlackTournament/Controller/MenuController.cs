using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;
using BlackTournament.Properties;

namespace BlackTournament.Controller
{
    public class MenuController : ControllerBase
    {
        private String _Message;
        private MainMenu _State;

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
            if (!String.IsNullOrWhiteSpace(_Message)) _State.DisplayPopupMessage(_Message);
            _State.Browse += State_BrowseClicked;
            _State.Host += State_HostClicked;
            // TODO: restore UI State
        }

        private void State_BrowseClicked()
        {
            // TODO : get available servers from client
            _State.OpenServerBrowser();
        }

        private void State_HostClicked()
        {
            var name = String.IsNullOrWhiteSpace(_State.Servername) ? $"{Settings.Default.PlayerName}'s Server" : _State.Servername;
            var port = _State.Port == 0 ? Net.Net.DEFAULT_PORT : _State.Port;

            if (_Game.Host(_State.Mapname, name, port)) _Game.Connect(Net.Net.DEFAULT_HOST, port);
            else _State.DisplayPopupMessage($"Failed to host {_State.Mapname} on {Net.Net.DEFAULT_HOST}:{port}. Is the map and port valid?");
        }

        protected override void StateReleased()
        {
            _State.Browse -= State_BrowseClicked;
            _State.Host -= State_HostClicked;
            _Message = null;
            _State = null;
        }
    }
}