using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.InputMapping;
using BlackTournament.GameStates;
using BlackTournament.Net;
using SFML.System;
using SFML.Window;

namespace BlackTournament.Controller
{
    public class MapController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private MapState _State;

        public MapController(Game game) : base(game)
        {
        }

        public void Activate(BlackTournamentClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");

            // Init
            _Client = client;
            Activate(_State = new MapState(_Game.Core, _Client.MapName));
        }

        private void AttachEvents()
        {
            _Client.UpdateReceived += UpdateReceived;
            _Client.ConnectionHasBeenLost += HandleConnectionLost;
            //_Client.ConnectionClosed += HandleConnectionClosed; // TODO

            Input.MouseMoved += Input_MouseMoved;
            _Game.InputManager.Action += _Client.ProcessGameAction;
        }

        private void DetachEvents()
        {
            _Client.UpdateReceived -= UpdateReceived;
            _Client.ConnectionHasBeenLost -= HandleConnectionLost;
            //_Client.ConnectionClosed -= HandleConnectionClosed; // FIXME

            Input.MouseMoved -= Input_MouseMoved;
            _Game.InputManager.Action -= _Client.ProcessGameAction;
        }

        private void Input_MouseMoved(Vector2f mousePosition)
        {
            var player = _Client._Players[_Client.Id]; // TODO : handle player without dictionary
            player.R = new Vector2f(_Game.Core.DeviceSize.X / 2, _Game.Core.DeviceSize.Y / 2).AngleTowards(mousePosition);
            _State.UpdatePosition(0, player.X, player.Y, player.R);
        }

        private void UpdateReceived()
        {
            // HACK HACK HACK
            var player = _Client._Players[_Client.Id]; // TODO : handle player without dictionary
            _State.UpdatePosition(0, player.X, player.Y, player.R);
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