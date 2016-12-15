using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat.InputMapping;
using BlackTournament.GameStates;
using BlackTournament.InputMapping;
using BlackTournament.Net;
using SFML.Window;

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

            // Init
            _Client = client;
            _State = new MapState(_Game.Core, _Client.MapName);

            var m = new InputMap<GameAction>();
            m.AddKeyboardMapping(Keyboard.Key.A, GameAction.MoveLeft);
            m.AddKeyboardMapping(Keyboard.Key.Left, GameAction.MoveLeft);
            m.AddKeyboardMapping(Keyboard.Key.Return, GameAction.Confirm);
            m.MappedOperationInvoked += M_MappedOperationInvoked;

            // Handle State Events
            _State.Ready += StateReady;
            _State.OnDestroy += ReleaseState;

            // Handle client events
            _Client.ConnectionLost += HandleConnectionLost;
            _Client.ConnectionClosed += HandleConnectionClosed;
            
            // Acivate State
            _Game.Core.StateManager.ChangeState(_State);
        }

        private void M_MappedOperationInvoked(GameAction obj)
        {
            Log.Debug(obj);
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