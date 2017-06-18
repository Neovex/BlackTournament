using System;

using SFML.System;

using BlackCoat;

using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Tmx;
using BlackTournament.Net.Data;
using System.Collections.Generic;
using BlackTournament.Systems;

namespace BlackTournament.Controller
{
    public class MapController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private MapState _State;

        private TmxMapper _MapData;
        private List<ClientPlayer> _Players;


        internal ClientPlayer Player { get { return _Client.Player; } }


        public MapController(Game game) : base(game)
        {
            _MapData = new TmxMapper();
            _Players = new List<ClientPlayer>();
        }

        protected override void StateReady()
        {
            _State.CreatePlayer(Player.Id, true);
            AttachEvents();
        }

        protected override void StateLoadingFailed()
        {
            StateReleased(); // add better error handling?
        }

        protected override void StateReleased()
        {
            DetachEvents();
            _Client = null;
            _State = null;
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
            // Connection Events
            _Client.ConnectionHasBeenLost += HandleConnectionLost;// _Client.ConnectionEstablished not required - connection already established when entering map state
            // Game Events
            _Client.Player.Fragged += HandlePlayerFragged;
            _Client.ChangeLevelReceived += HandleServerMapChange;
            _Client.MessageReceived += HandleTextMessage;
            _Client.UpdateReceived += UpdateReceived;
            _Client.UserJoined += HandleUserJoined;
            _Client.UserLeft += HandleUserLeft;
            // System Events
            Input.MouseMoved += Input_MouseMoved;
            _Game.InputMapper.Action += HandleInput;
        }

        private void DetachEvents()
        {
            // Connection Events
            _Client.ConnectionHasBeenLost -= HandleConnectionLost;
            // Game Events
            _Client.Player.Fragged -= HandlePlayerFragged;
            _Client.ChangeLevelReceived -= HandleServerMapChange;
            _Client.MessageReceived -= HandleTextMessage;
            _Client.UpdateReceived -= UpdateReceived;
            _Client.UserJoined -= HandleUserJoined;
            _Client.UserLeft -= HandleUserLeft;
            // System Events
            Input.MouseMoved -= Input_MouseMoved;
            _Game.InputMapper.Action -= HandleInput;
        }

        private void HandleInput(GameAction action, bool activate)
        {
            var move = _State.ViewMovement;
            switch (action)
            {
                case GameAction.Confirm:
                    // needed?
                    break;
                case GameAction.Cancel:
                    // open menu
                    break;
                case GameAction.ShowStats:
                    // TODO
                    break;
                case GameAction.MoveUp: // auslagern?
                    move.Y = activate ? -1 : 0;
                    break;
                case GameAction.MoveDown:
                    move.Y = activate ?  1 : 0;
                    break;
                case GameAction.MoveLeft:
                    move.X = activate ? -1 : 0;
                    break;
                case GameAction.MoveRight:
                    move.X = activate ?  1 : 0;
                    break;
            }
            // Move view only when player is dead
            _State.ViewMovement = Player.IsAlive ? new Vector2f() : move;

            // Hand input to the server 4 processing game-logic
            _Client.ProcessGameAction(action, activate);
        }

        private void HandlePlayerFragged(ClientPlayer player)
        {
            if(player == Player)
            {
                // dang! we fragged
            }
            else
            {
                // another one bites the dust
            }
        }

        private void HandleServerMapChange()
        {
            Log.Debug("TODO: HandleServerMapChange");
        }

        private void HandleTextMessage(ClientPlayer player, string msg)
        {
            Log.Debug("TODO: HandleTextMessage", msg);
        }

        private void UpdateReceived()
        {
            _State.UpdateEntity(Player.Id, Player.Position, Player.Rotation, Player.Health > 0);
            _State.RotatePlayer(_Client.PlayerRotation); // reset state player rotation to prevent lag flickering
            if (Player.IsAlive) _State.FocusPlayer(); // move camera to player

            // TODO : update other entities
        }

        private void HandleUserJoined(ClientPlayer player)
        {
            _Players.Add(player);
            player.Fragged += HandlePlayerFragged;
            _State.CreatePlayer(player.Id);
        }

        private void HandleUserLeft(ClientPlayer player)
        {
            _Players.Remove(player);
            player.Fragged -= HandlePlayerFragged;
            _State.Destroy(player.Id);
        }

        private void Input_MouseMoved(Vector2f mousePosition)
        {
            _Client.PlayerRotation = (_Game.Core.DeviceSize / 2).ToVector2f().AngleTowards(mousePosition);
            _State.RotatePlayer(_Client.PlayerRotation); // update state
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