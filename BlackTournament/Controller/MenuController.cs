using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BlackTournament.Scenes;
using BlackTournament.Net;
using BlackTournament.Net.Data;
using BlackTournament.Properties;

namespace BlackTournament.Controller
{
    public class MenuController : ControllerBase
    {
        private String _Message;
        private MainMenu _Scene;
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

            Activate(_Scene = new MainMenu(_Game.Core, maps));
        }

        protected override void SceneLoadingFailed()
        {
            throw new Exception("FUBAR");
        }

        protected override void SceneReady()
        {
            if (!String.IsNullOrWhiteSpace(_Message)) _Scene.DisplayPopupMessage(_Message);
            _Scene.ServerBrowserOpen += HandleSceneServerBrowserOpen;
            _Scene.ServerBrowserRefresh += HandleSceneServerBrowserRefresh;
            _Scene.JoinServer += _Game.Connect;
            _Scene.StartHosting += HandleSceneStartHosting;
            _Scene.DirectConnect += _Game.Connect;
        }

        private void HandleLanServersUpdated()
        {
            var l = _NetworkManagementClient.LanServers.ToArray();
            Log.Debug("Refreshing server list with", l.Length, "Servers");
            _Scene.UpdateServerList(l);
        }

        private void HandleWanServersUpdated()
        {
            // TODO : Implement this when management server is functional
            throw new NotImplementedException();
        }

        private void HandleSceneServerBrowserOpen()
        {
            _NetworkManagementClient = new NetworkManagementClient();
            _NetworkManagementClient.LanServersUpdated += HandleLanServersUpdated;
            _NetworkManagementClient.WanServersUpdated += HandleWanServersUpdated;
            _NetworkManagementClient.DiscoverLanServers(Net.Net.DEFAULT_PORT);
            HandleLanServersUpdated(); // HACK

            _Game.Core.OnUpdate += HandleCoreUpdate;

            _Scene.OpenServerBrowser(true);
        }

        private void HandleCoreUpdate(float deltaT)
        {
            _NetworkManagementClient.ProcessMessages();
        }

        private void HandleSceneServerBrowserRefresh()
        {
            _NetworkManagementClient.DiscoverLanServers(Net.Net.DEFAULT_PORT, false);
        }

        private void HandleSceneStartHosting(string mapName, string serverName, int port)
        {
            var name = String.IsNullOrWhiteSpace(serverName) ? $"{Settings.Default.PlayerName}'s Server" : serverName;

            if (_Game.Host(port, mapName, name)) _Game.Connect(Net.Net.DEFAULT_HOST, port);
            else _Scene.DisplayPopupMessage($"Failed to host {mapName} on {Net.Net.DEFAULT_HOST}:{port}. Is the map and port valid?");
        }

        protected override void SceneReleased()
        {
            if (_NetworkManagementClient != null)
            {
                _NetworkManagementClient.LanServersUpdated -= HandleLanServersUpdated;
                _NetworkManagementClient.WanServersUpdated -= HandleWanServersUpdated;
                _NetworkManagementClient.Dispose();
                _NetworkManagementClient = null;

                _Game.Core.OnUpdate -= HandleCoreUpdate;
            }

            _Scene.ServerBrowserOpen -= HandleSceneServerBrowserOpen;
            _Scene.ServerBrowserRefresh -= HandleSceneServerBrowserRefresh;
            _Scene.JoinServer -= _Game.Connect;
            _Scene.StartHosting -= HandleSceneStartHosting;
            _Scene.DirectConnect -= _Game.Connect;
            _Scene = null;
        }
    }
}