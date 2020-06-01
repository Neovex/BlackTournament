using System;
using BlackNet.Server;
using BlackTournament.Tmx;
using BlackTournament.Net;
using BlackTournament.Scenes;

namespace BlackTournament.Controller
{
    public class ConnectController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private ConnectScene _Scene;

        private ServerInfo _ServerInfo;
        private (String Host, Int32 Port) _AltServerInfo;


        public ConnectController(Game game) : base(game)
        {
        }


        public void Activate(BlackTournamentClient client, String host, Int32 port)
        {
            if (_Client != null || _Scene != null) throw new Exception("Invalid Controller State");
            _Client = client;
            if (String.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            _AltServerInfo.Host = host;
            _AltServerInfo.Port = port;

            // Create then switch to Connect Scene
            Activate(_Scene = new ConnectScene(_Game.Core, host));
        }

        public void Activate(BlackTournamentClient client, ServerInfo host)
        {
            if (_Client != null || _Scene != null) throw new Exception("Invalid Controller State");
            _Client = client;
            _ServerInfo = host ?? throw new ArgumentNullException(nameof(host));

            // Create then switch to Connect Scene
            var displayName = String.IsNullOrWhiteSpace(_ServerInfo.Name) ? host.EndPoint.ToString() : _ServerInfo.Name;
            Activate(_Scene = new ConnectScene(_Game.Core, displayName));
        }

        protected override void SceneReady()
        {
            // Connect to Host
            _Client.ChangeLevelReceived += LevelReady;
            _Client.OnDisconnect += ConnectionFailed;
            bool connected;
            if (_ServerInfo != null) connected = _Client.Connect(_ServerInfo.EndPoint);
            else connected = _Client.Connect(_AltServerInfo.Host, _AltServerInfo.Port);
            if (!connected) _Game.MenuController.Activate(_Client.LastError);
        }
        protected override void SceneLoadingFailed()
        {
            SceneReleased();
        }
        protected override void SceneReleased()
        {
            _Client.ChangeLevelReceived -= LevelReady;
            _Client.OnDisconnect -= ConnectionFailed;
            _Client = null;
            _Scene = null;
        }

        private void LevelReady()
        {
            Log.Debug("Client Ready");
            if (TmxMapper.MapIsAvailable(_Client.MapName))
            {
                _Game.MapController.Activate(_Client);
            }
            else
            {
                var mapName = _Client.MapName;
                Log.Debug("Map", mapName, "not available on client side - aborting connection");
                _Client.Disconnect();
                _Game.MenuController.Activate($"Error: you do not have the map '{mapName}'. Connection aborted.");//$
            }
        }

        private void ConnectionFailed()
        {
            _Game.MenuController.Activate("Connection failed");//$
        }
    }
}