using System;
using BlackCoat;
using BlackTournament.GameStates;
using BlackTournament.Net;
using SFML.System;

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
            _Game.InputMapper.Action += _Client.ProcessGameAction;
        }

        private void DetachEvents()
        {
            _Client.UpdateReceived -= UpdateReceived;
            _Client.ConnectionHasBeenLost -= HandleConnectionLost;
            //_Client.ConnectionClosed -= HandleConnectionClosed; // FIXME

            Input.MouseMoved -= Input_MouseMoved;
            _Game.InputMapper.Action -= _Client.ProcessGameAction;
        }

        private void Input_MouseMoved(Vector2f mousePosition)
        {
            var player = _Client.Player;
            player.R = new Vector2f(_Game.Core.DeviceSize.X / 2, _Game.Core.DeviceSize.Y / 2).AngleTowards(mousePosition);
            //HACK?
            _State.Rotate(player.R);
        }

        private void UpdateReceived()
        {
            // HACK HACK HACK
            var player = _Client.Player;
            player.R = new Vector2f(_Game.Core.DeviceSize.X / 2, _Game.Core.DeviceSize.Y / 2).AngleTowards(Input.MousePosition);
            _State.UpdatePosition(0, player.X, player.Y);
            // rotate only other players
        }

        protected override void StateReady()
        {
            AttachEvents();

            // TODO : feed state (aka view) with data here and only now
            // register additional to state events
        }

        protected override void StateLoadingFailed()
        {
            StateReleased();
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