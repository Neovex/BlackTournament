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


        internal ClientPlayer LocalPlayer { get { return _Client.Player; } }


        public MapController(Game game) : base(game)
        {
            _MapData = new TmxMapper(Net.Net.GetNextId);
            _Players = new List<ClientPlayer>();
        }

        protected override void StateReady()
        {
            foreach (var player in _Client.Players)
            {
                HandleUserJoined(player);
            }

            foreach(var pickup in _Client.Pickups)
            {
                _State.CreatePickup(pickup.Id, pickup.Type, pickup.Position, pickup.Active);
            }
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
            if (_Client != null || _State != null) throw new InvalidStateException();

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
            // _Client.ConnectionEstablished not required - connection already established when entering map state
            _Client.OnDisconnect += HandleConnectionLost;
            // Game Events
            _Client.Player.Fragged += HandlePlayerFragged;
            _Client.ChangeLevelReceived += HandleServerMapChange;
            _Client.MessageReceived += HandleTextMessage;
            _Client.UpdateReceived += UpdateReceived;
            _Client.UserJoined += HandleUserJoined;
            _Client.UserLeft += HandleUserLeft;
            _Client.ShotFired += HandleShotFired;
            _Client.ShotRemoved += HandleShotRemoved;
            foreach (var pickup in _Client.Pickups)
            {
                pickup.ActiveStateChanged += HandlePickupStateChanged;
            }
            // System Events
            _Game.InputMapper.Input.MouseMoved += Input_MouseMoved;
            _Game.InputMapper.Action += HandleInput;
        }

        private void DetachEvents()
        {
            // Connection Events
            _Client.OnDisconnect -= HandleConnectionLost;
            // Game Events
            _Client.Player.Fragged -= HandlePlayerFragged;
            _Client.ChangeLevelReceived -= HandleServerMapChange;
            _Client.MessageReceived -= HandleTextMessage;
            _Client.UpdateReceived -= UpdateReceived;
            _Client.UserJoined -= HandleUserJoined;
            _Client.UserLeft -= HandleUserLeft;
            _Client.ShotFired -= HandleShotFired;
            _Client.ShotRemoved -= HandleShotRemoved;
            foreach (var pickup in _Client.Pickups)
            {
                pickup.ActiveStateChanged -= HandlePickupStateChanged;
            }
            // System Events
            _Game.InputMapper.Input.MouseMoved -= Input_MouseMoved;
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
            _State.ViewMovement = LocalPlayer.IsAlive ? new Vector2f() : move;

            // Hand input to the server 4 processing game-logic
            _Client.ProcessGameAction(action, activate);
        }

        private void HandlePlayerFragged(ClientPlayer player)
        {
            if(player == LocalPlayer)
            {
                // dang! we got fragged
            }
            else
            {
                // another one bites the dust, uh yeah
            }
        }

        private void HandlePickupStateChanged(Pickup pickup)
        {
            _State.UpdateEntity(pickup.Id, pickup.Position, 0, pickup.Active);
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
            foreach (var player in _Client.Players)
            {
                _State.UpdateEntity(player.Id, player.Position, player.Rotation, player.IsAlive);
            }

            _State.RotatePlayer(_Client.PlayerRotation); // reset state player rotation to prevent lag flickering
            if (LocalPlayer.IsAlive) _State.FocusPlayer(); // move camera to player

            foreach (var shot in _Client.Shots)
            {
                _State.UpdateEntity(shot.Id, shot.Position, shot.Direction, true);
            }

            foreach (var efx in _Client.Effects)
            {
                _State.CreateEffect(efx.EffectType, efx.Position, efx.Rotation, efx.Source, efx.Primary, efx.Size);
            }
        }

        private void HandleUserJoined(ClientPlayer player)
        {
            _Players.Add(player);
            player.Fragged += HandlePlayerFragged;
            _State.CreatePlayer(player.Id, player.Id == LocalPlayer.Id);
        }

        private void HandleUserLeft(ClientPlayer player)
        {
            _Players.Remove(player);
            player.Fragged -= HandlePlayerFragged;
            _State.DestroyEntity(player.Id);
        }

        private void HandleShotFired(Shot shot)
        {
            _State.CreateProjectile(shot.Id, shot.SourceWeapon, shot.Position, shot.Direction, shot.Primary);
        }

        private void HandleShotRemoved(Shot shot)
        {
            _State.DestroyEntity(shot.Id);
        }

        private void Input_MouseMoved(Vector2f mousePosition)
        {
            _Client.PlayerRotation = (_Game.Core.DeviceSize / 2).ToVector2f().AngleTowards(mousePosition)-2;
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