using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;
using BlackTournament.Net;

namespace BlackTournament.Controller
{
    public class MapController
    {
        private Game _Game;
        private GameClient _Client;
        private MapState _State;

        public MapController(Game game)
        {
            _Game = game;
        }

        public void Activate(GameClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");
            _Client = client;

            // Build and switch to Map State
            _State = new MapState(_Game.Core, _Client.MapName);
            _State.Ready += StateReady;
            _State.OnDestroy += ReleaseState;
            _Game.Core.StateManager.ChangeState(_State);

            // Handle connection loss
            _Client.ConnectionLost += HandleConnectionLost;
            _Client.ConnectionClosed += HandleConnectionClosed;
            
        }

        private void StateReady()
        {
            // TODO : feed state (aka view) with data here and only now
            // register to state events
        }

        private void ReleaseState()
        {
            // deregister from state events
            _State.Ready -= StateReady;
            _State.OnDestroy -= ReleaseState;
            _State = null;

            _Client.ConnectionLost -= HandleConnectionLost;
            _Client.ConnectionClosed -= HandleConnectionClosed;
            _Client = null;
        }

        private void HandleConnectionLost()
        {
            _Game.MenuController.Activate("Connection Lost");//$
        }

        private void HandleConnectionClosed()
        {
            _Game.MenuController.Activate();
        }
    }
}