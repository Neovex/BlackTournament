using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Net.Data;
using BlackTournament.Properties;

namespace BlackTournament.Controller
{
    public class MenuController : ControllerBase
    {
        private String _Message;
        private MainMenu _State;
        private NetworkManagementClient _NetworkManagementClient;

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
            _State.ServerBrowserOpen += HandleStateServerBrowserOpen;
            _State.ServerBrowserRefresh += HandleStateServerBrowserRefresh;
            _State.JoinServer += _Game.Connect;
            _State.StartHosting += HandleStateStartHosting;
        }

        private void HandleLanServersUpdated()
        {
            var l = _NetworkManagementClient.LanServers.ToArray();
            Log.Debug("Refreshing server list with", l.Length, "Servers");
            _State.UpdateServerList(l);
        }

        private void HandleWanServersUpdated()
        {
            // TODO : Implement this when management server is functional
            throw new NotImplementedException();
        }

        private void HandleStateServerBrowserOpen()
        {
            _NetworkManagementClient = new NetworkManagementClient();
            _NetworkManagementClient.LanServersUpdated += HandleLanServersUpdated;
            _NetworkManagementClient.WanServersUpdated += HandleWanServersUpdated;
            _NetworkManagementClient.DiscoverLanServers(Net.Net.DEFAULT_PORT);
            HandleLanServersUpdated(); // HACK

            _Game.Core.OnUpdate += HandleCoreUpdate;

            _State.OpenServerBrowser(true);
        }

        private void HandleCoreUpdate(float deltaT)
        {
            _NetworkManagementClient.ProcessMessages();
        }

        private void HandleStateServerBrowserRefresh()
        {
            _NetworkManagementClient.DiscoverLanServers(Net.Net.DEFAULT_PORT, false);
        }

        private void HandleStateStartHosting()
        {
            var name = String.IsNullOrWhiteSpace(_State.Servername) ? $"{Settings.Default.PlayerName}'s Server" : _State.Servername;
            var port = _State.Port < 1 ? Net.Net.DEFAULT_PORT : _State.Port;

            if (_Game.Host(port, _State.Mapname, name)) _Game.Connect(Net.Net.DEFAULT_HOST, port);
            else _State.DisplayPopupMessage($"Failed to host {_State.Mapname} on {Net.Net.DEFAULT_HOST}:{port}. Is the map and port valid?");
        }

        protected override void StateReleased()
        {
            if (_NetworkManagementClient != null)
            {
                _NetworkManagementClient.LanServersUpdated -= HandleLanServersUpdated;
                _NetworkManagementClient.WanServersUpdated -= HandleWanServersUpdated;
                _NetworkManagementClient.Dispose();
                _NetworkManagementClient = null;

                _Game.Core.OnUpdate -= HandleCoreUpdate;
            }

            _State.ServerBrowserOpen -= HandleStateServerBrowserOpen;
            _State.ServerBrowserRefresh -= HandleStateServerBrowserRefresh;
            _State.JoinServer -= _Game.Connect;
            _State.StartHosting -= HandleStateStartHosting;
            _State = null;
        }
    }
}