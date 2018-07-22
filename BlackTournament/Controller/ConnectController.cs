using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Properties;

namespace BlackTournament.Controller
{
    public class ConnectController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private ConnectState _State;

        //public event Action ConnectionEstablished = () => { }; //?


        public ConnectController(Game game) : base(game)
        {
        }

        public void Activate(BlackTournamentClient client,String host, Int32 port)
        {
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");
            _Client = client;

            // Build and switch to Connect State
            Activate(_State = new ConnectState(_Game.Core, host));

            // Connect to Host
            _Client.ChangeLevelReceived += LevelReady;
            _Client.OnDisconnect += ConnectionFailed;
            _Client.Connect(host, port);
        }

        protected override void StateReady()
        {
            // TODO : feed state (aka view) with data here and only now
        }
        protected override void StateLoadingFailed()
        {
            StateReleased();
        }
        protected override void StateReleased()
        {
            _Client.ChangeLevelReceived -= LevelReady;
            _Client.OnDisconnect -= ConnectionFailed;
            _Client = null;
            _State = null;
        }

        private void LevelReady()
        {
            Log.Debug("Client Ready");
            if (MapIsAvailable(_Client.MapName))
            {
                _Game.MapController.Activate(_Client);
            }
            else
            {
                var mapName = _Client.MapName;
                Log.Debug("Map", mapName, "not available on client side - aborting connection");
                _Client.Disconnect(); // TODO : test this block! might have a race between disconnect event activation below
                _Game.MenuController.Activate($"Error: you do not have the map {mapName}. Connection aborted.");//$
            }
        }

        private bool MapIsAvailable(string mapName)
        {
            Log.Fatal("Skipped map validity check for", mapName); // TODO : implement and maybe move to game or another map collection handler that checks all map at game load
            return true;
        }

        private void ConnectionFailed()
        {
            _Game.MenuController.Activate("Connection Failed");//$
        }
    }
}