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
    public class MapController : ControllerBase
    {
        private GameClient _Client;
        private MapState _State;

        public MapController(Game game) : base(game)
        {
        }

        public void Activate(GameClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");

            // Init
            _Client = client;
            Activate(_State = new MapState(_Game.Core, _Client.MapName));

            // Input map test - lets find a better place for this - file?
            var m = new InputMap<GameAction>();
            m.AddKeyboardMapping(Keyboard.Key.A, GameAction.MoveLeft);
            m.AddKeyboardMapping(Keyboard.Key.Left, GameAction.MoveLeft);
            m.AddKeyboardMapping(Keyboard.Key.Return, GameAction.Confirm);
            m.MappedOperationInvoked += HandleInput; // CH?

            // Handle client events
            _Client.ConnectionLost += HandleConnectionLost;
            _Client.ConnectionClosed += HandleConnectionClosed;
            
            // Acivate State
            _Game.Core.StateManager.ChangeState(_State);
        }

        private void HandleInput(GameAction obj)
        {
            Log.Debug(obj);
        }

        protected override void StateReady()
        {
            // TODO : feed state (aka view) with data here and only now
            // register additional to state events
        }

        protected override void StateReleased()
        {
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