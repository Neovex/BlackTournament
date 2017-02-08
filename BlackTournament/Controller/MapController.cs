using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat.InputMapping;
using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Net.Lid;
using SFML.Window;

namespace BlackTournament.Controller
{
    public class MapController : ControllerBase
    {
        private LClient _Client;
        private MapState _State;

        public MapController(Game game) : base(game)
        {
        }

        public void Activate(LClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");

            // Init
            _Client = client;
            Activate(_State = new MapState(_Game.Core, _Client.MapName));
        }

        private void AttachEvents()
        {
            _Client.ConnectionLost += HandleConnectionLost;
            _Client.ConnectionClosed += HandleConnectionClosed;

            _Client.UpdatePositionReceived += UpdatePosition;

            _Game.InputManager.Action += _Client.ProcessGameAction;
        }

        private void UpdatePosition(int id, float x, float y, float angle)
        {
            _State.UpdatePosition(id, x, y, angle);
        }

        private void DetachEvents()
        {
            _Client.ConnectionLost -= HandleConnectionLost;
            _Client.ConnectionClosed -= HandleConnectionClosed;

            _Game.InputManager.Action -= _Client.ProcessGameAction;
        }

        protected override void StateReady()
        {
            AttachEvents();
            // TODO : feed state (aka view) with data here and only now
            // register additional to state events
        }

        protected override void StateReleased()
        {
            DetachEvents();
            _Client = null;
            _State = null;
        }

        private void ExitToMenue() // TODO attach to proper input - and or view event
        {
            if (_Client.IsAdmin)
            {
                _Client.StopServer();
            }
            else
            {
                _Client.Disconnect();
            }
        }

        private void HandleConnectionLost()
        {
            DetachEvents();
            _Game.MenuController.Activate("Connection Lost");//$
        }

        private void HandleConnectionClosed()
        {
            DetachEvents();
            _Game.MenuController.Activate();
        }
    }
}