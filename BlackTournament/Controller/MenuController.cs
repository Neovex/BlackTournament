using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

            // Load available Maps
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Maps");
            var maps = Directory.EnumerateFiles(path, "*.tmx").
                       Select(f => Path.GetFileNameWithoutExtension(f)).
                       ToArray();

            Activate(_State = new MainMenu(_Game.Core, maps));
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
            _State.DirectConnect += _Game.Connect;
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

        private void HandleStateStartHosting(string mapName, string serverName, int port)
        {
            var name = String.IsNullOrWhiteSpace(serverName) ? $"{Settings.Default.PlayerName}'s Server" : serverName;

            if (_Game.Host(port, mapName, name)) _Game.Connect(Net.Net.DEFAULT_HOST, port);
            else _State.DisplayPopupMessage($"Failed to host {mapName} on {Net.Net.DEFAULT_HOST}:{port}. Is the map and port valid?");
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
            _State.DirectConnect -= _Game.Connect;
            _State = null;
        }
    }
}