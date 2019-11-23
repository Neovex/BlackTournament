using System;
using System.Collections.Generic;

using SFML.System;
using BlackCoat;

using BlackTournament.Scenes;
using BlackTournament.Net;
using BlackTournament.Tmx;
using BlackTournament.Net.Data;
using BlackTournament.Systems;

namespace BlackTournament.Controller
{
    public class MapController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private MapScene _Scene;


        private ClientPlayer LocalPlayer { get { return _Client.Player; } }


        public MapController(Game game) : base(game)
        {
        }

        public void Activate(BlackTournamentClient client)
        {
            if (_Client != null || _Scene != null) throw new InvalidStateException();
            _Client = client ?? throw new ArgumentNullException(nameof(client));

            var map = new TmxMapper();
            if (map.Load(_Client.MapName, _Game.Core.CollisionSystem))
            {
                base.Activate(_Scene = new MapScene(_Game.Core, map));
            }
            else
            {
                _Client.Disconnect();
                _Game.MenuController.Activate($"Failed to initialize map {_Client.MapName}");//$
            }
        }

        protected override void SceneReady()
        {
            foreach (var player in _Client.Players)
            {
                HandleUserJoined(player);
            }

            foreach(var pickup in _Client.Pickups)
            {
                _Scene.CreatePickup(pickup.Id, pickup.Type, pickup.Position, pickup.Active);
            }
            AttachEvents();
        }

        protected override void SceneLoadingFailed()
        {
            _Game.MenuController.Activate($"Failed to load map {_Client.MapName}");//$
        }

        protected override void SceneReleased()
        {
            DetachEvents();
            _Client = null;
            _Scene = null;
        }


        private void AttachEvents()
        {
            // Connection Events
            // _Client.ConnectionEstablished not required - connection already established when entering map state
            _Client.OnDisconnect += HandleConnectionLost;
            // Game Events
            //_Client.Player.Fragged += HandlePlayerFragged; Added by _manual_ invocation of player joined
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
            _Client.Player.Fragged -= HandlePlayerFragged; // See also notes under AttachEvents
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
            var move = _Scene.ViewMovement;
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
            _Scene.ViewMovement = LocalPlayer.IsAlive ? default(Vector2f) : move;

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
            _Scene.UpdateEntity(pickup.Id, pickup.Position, 0, pickup.Active);
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
                _Scene.UpdateEntity(player.Id, player.Position, player.Rotation, player.IsAlive);
            }

            if (LocalPlayer.IsAlive) _Scene.FocusPlayer(); // move camera to player
            // Update Player
            _Scene.RotatePlayer(_Client.PlayerRotation); // reset state player rotation to prevent lag flickering
            _Scene.HUD.SetPlayerInfoVisibility(LocalPlayer.IsAlive);
            _Scene.HUD.SetPlayerWeapon(LocalPlayer.CurrentWeaponType);
            _Scene.HUD.Health = (int)LocalPlayer.Health;
            _Scene.HUD.Shield = (int)LocalPlayer.Shield;
            

            foreach (var shot in _Client.Shots)
            {
                _Scene.UpdateEntity(shot.Id, shot.Position, shot.Direction, true);
            }

            foreach (var efx in _Client.Effects)
            {
                _Scene.CreateEffect(efx.EffectType, efx.Position, efx.Rotation, efx.Source, efx.Primary, efx.Size);
            }
        }

        private void HandleUserJoined(ClientPlayer player)
        {
            player.Fragged += HandlePlayerFragged;
            _Scene.CreatePlayer(player.Id, player.Id == LocalPlayer.Id);
        }

        private void HandleUserLeft(ClientPlayer player)
        {
            player.Fragged -= HandlePlayerFragged;
            _Scene.DestroyEntity(player.Id);
        }

        private void HandleShotFired(Shot shot)
        {
            _Scene.CreateProjectile(shot.Id, shot.SourceWeapon, shot.Position, shot.Direction, shot.Primary);
        }

        private void HandleShotRemoved(Shot shot)
        {
            _Scene.DestroyEntity(shot.Id);
        }

        private void Input_MouseMoved(Vector2f mousePosition)
        {
            _Client.PlayerRotation = (_Game.Core.DeviceSize / 2).AngleTowards(mousePosition) - 2;
            _Client.PlayerRotation = MathHelper.ValidateAngle(_Client.PlayerRotation);
            _Scene.RotatePlayer(_Client.PlayerRotation); // update scene
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