using System;

using SFML.System;

using BlackCoat;

using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Tmx;

namespace BlackTournament.Controller
{
    public class MapController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private MapState _State;

        private TmxMapper _MapData;


        public MapController(Game game) : base(game)
        {
            _MapData = new TmxMapper();
        }


        public void Activate(BlackTournamentClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");

            // Init
            _Client = client;
            if (_MapData.Load(_Client.MapName, _Game.Core.CollisionSystem))
            {
                Activate(_State = new MapState(_Game.Core, _MapData));
            }
            else
            {
                _Client.Disconnect();
                _Game.MenuController.Activate($"Failed to load map {_Client.MapName}");//$
            }
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
            _State.Rotate(_Client.Player.R = new Vector2f(_Game.Core.DeviceSize.X / 2, _Game.Core.DeviceSize.Y / 2).AngleTowards(mousePosition));
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