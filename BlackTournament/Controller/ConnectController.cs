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
    public class ConnectController
    {
        private Game _Game;
        private GameClient _Client;
        private ConnectState _State;

        public event Action ConnectionEstablished = () => { };

        
        public ConnectController(Game game)
        {
            _Game = game;
        }

        public void Activate(GameClient client)
        {
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");
            _Client = client;

            // Build and switch to Connect State
            _State = new ConnectState(_Game.Core, _Client.Host);
            _State.Ready += ConnectStateReady;
            _State.OnDestroy += ReleaseState;
            _Game.Core.StateManager.ChangeState(_State);

            // Connect to Host
            _Client.ConnectionEstablished += Connected;
            _Client.ConnectionFailed += ConnectionFailed;
            _Client.Connect(); // check: make async?
        }

        private void ConnectStateReady()
        {
            // TODO : feed state (aka view) with data here and only now
        }

        private void ReleaseState()
        {
            // deregister from state events
            _State.Ready -= ConnectStateReady;
            _State.OnDestroy -= ReleaseState;
            _State = null;

            _Client.ConnectionEstablished -= Connected;
            _Client.ConnectionFailed -= ConnectionFailed;
            _Client = null;
        }

        private void Connected()
        {
            _Game.Core.AnimationManager.Wait(2, a => { _Game.MapController.Activate(_Client); }); // TODO: move timeout to map controller?
        }

        private void ConnectionFailed()
        {
            _Game.MenuController.Activate("Connection Failed");//$
        }
    }
}