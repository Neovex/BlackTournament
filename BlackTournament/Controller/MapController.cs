using System;
using System.Linq;
using System.Collections.Generic;

using SFML.System;
using BlackCoat;

using BlackTournament.Scenes;
using BlackTournament.Net;
using BlackTournament.Tmx;
using BlackTournament.Net.Data;
using BlackTournament.InputMaps;

namespace BlackTournament.Controller
{
    public class MapController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private MapScene _Scene;
        private bool _InChat;

        private GameInputMap _GameInput;
        private UIInputMap _UiInput;


        private ClientPlayer LocalPlayer { get { return _Client.Player; } }


        public MapController(Game game) : base(game)
        {
        }

        public void Activate(BlackTournamentClient client)
        {
            // Set Client
            if (_Client != null || _Scene != null) throw new InvalidStateException();
            _Client = client ?? throw new ArgumentNullException(nameof(client));

            // Load Map
            var map = new TmxMapper();
            if (map.Load(_Client.MapName, _Game.Core.CollisionSystem))
            {
                // Init Input
                _GameInput = new GameInputMap(new Input(_Game.Core));
                _UiInput = new UIInputMap(new GameInputMap(new Input(_Game.Core, false)));
                // Activate Scene
                base.Activate(_Scene = new MapScene(_Game.Core, map, _UiInput));
            }
            else
            {
                _Client.Disconnect();
                _Game.MenuController.Activate($"Failed to initialize map {_Client.MapName}");
            }
        }

        protected override void SceneReady()
        {// Prepare Map
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
            _Client.ChangeLevelReceived += HandleServerMapChange;
            _Client.MessageReceived += HandleTextMessage;
            _Client.UpdateReceived += HandleUpdateReceived;
            _Client.UserJoined += HandleUserJoined;
            _Client.UserLeft += HandleUserLeft;
            _Client.ShotFired += HandleShotFired;
            _Client.ShotRemoved += HandleShotRemoved;
            foreach (var pickup in _Client.Pickups)
            {
                pickup.ActiveStateChanged += HandlePickupStateChanged;
            }

            // System Events
            _GameInput.Input.MouseMoved += Input_MouseMoved;
            _GameInput.MappedOperationInvoked += HandleInput;
        }

        private void DetachEvents()
        {
            // Connection Events
            _Client.OnDisconnect -= HandleConnectionLost;
            // Game Events
            _Client.ChangeLevelReceived -= HandleServerMapChange;
            _Client.MessageReceived -= HandleTextMessage;
            _Client.UpdateReceived -= HandleUpdateReceived;
            _Client.UserJoined -= HandleUserJoined;
            _Client.UserLeft -= HandleUserLeft;
            _Client.ShotFired -= HandleShotFired;
            _Client.ShotRemoved -= HandleShotRemoved;
            foreach (var pickup in _Client.Pickups)
            {
                pickup.ActiveStateChanged -= HandlePickupStateChanged;
            }
            // System Events
            _GameInput.Input.MouseMoved -= Input_MouseMoved;
            _GameInput.MappedOperationInvoked -= HandleInput;
            _GameInput.Dispose();
            _GameInput = null;
        }

        private void Input_MouseMoved(Vector2f mousePosition)
        {
            if (_InChat) return;
            _Client.PlayerRotation = MathHelper.ValidateAngle((_Game.Core.DeviceSize / 2).AngleTowards(mousePosition) - 2);
            _Scene.RotatePlayer(_Client.PlayerRotation); // update scene
        }

        private void HandleInput(GameAction action, bool activate)
        {
            var move = _Scene.ViewMovement;
            switch (action)
            {
                case GameAction.Confirm:
                    if(activate) ToggleChat();
                    break;
                case GameAction.Cancel:
                    // close chat & open menu
                    if (activate && _InChat) ToggleChat(); // TODO : else menu
                    break;
                case GameAction.ShowStats:
                    _Scene.HUD.ScoreBoard.Visible = activate;
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

            if (!_InChat)
            {
                // Move view only when player is dead
                _Scene.ViewMovement = LocalPlayer.IsAlive ? default(Vector2f) : move;

                // Hand input to the server 4 processing game-logic
                _Client.ProcessGameAction(action, activate);
            }
        }

        private void ToggleChat()
        {
            if (_InChat)
            {
                if (!String.IsNullOrWhiteSpace(_Scene.HUD.ChatMessage)) _Client.SendMessage(_Scene.HUD.ChatMessage);
                _Scene.HUD.DisableChat();
            }
            else
            {
                _Scene.HUD.EnableChat();
            }
            _InChat = !_InChat;
        }

        private void HandlePickupStateChanged(Pickup pickup)
        {
            _Scene.UpdateEntity(pickup.Id, pickup.Position, 0, pickup.Active);
        }

        private void HandleServerMapChange()
        {
            Log.Debug("TODO: HandleServerMapChange"); // TODO
        }

        private void HandleTextMessage(bool isSystemMessage, string msg)
        {
            _Scene.HUD.ShowMessage(isSystemMessage, msg);
        }

        private void HandleUpdateReceived()
        {
            foreach (var player in _Client.Players)
            {
                _Scene.UpdateEntity(player.Id, player.Position, player.Rotation, player.IsAlive);
            }

            if (LocalPlayer.IsAlive) _Scene.FocusPlayer(); // move camera to player
            // Update Player
            _Scene.RotatePlayer(_Client.PlayerRotation); // reset state player rotation to prevent lag flickering

            foreach (var shot in _Client.Shots)
            {
                _Scene.UpdateEntity(shot.Id, shot.Position, shot.Direction, true);
            }

            foreach (var efx in _Client.Effects)
            {
                _Scene.CreateEffect(efx.EffectType, efx.Position, efx.Rotation, efx.Source, efx.Primary, efx.Size);
            }

            // HUD
            _Scene.HUD.Alive = LocalPlayer.IsAlive;
            _Scene.HUD.Score = LocalPlayer.Score;
            _Scene.HUD.Time = TimeSpan.FromSeconds(_Client.GameTime);
            _Scene.HUD.Rank = _Client.Players.GroupBy(p => p.Score).TakeWhile(g => !g.Contains(LocalPlayer)).Count() + 1;
            _Scene.HUD.TotalPlayers = _Client.PlayerCount;
            _Scene.HUD.Health = LocalPlayer.Health;
            _Scene.HUD.Shield = LocalPlayer.Shield;
            _Scene.HUD.SetPlayerWeapons(LocalPlayer.CurrentWeaponType, LocalPlayer.Weapons.Values.AsEnumerable());
            _Scene.HUD.ScoreBoard.Update(_Client.Players);
        }

        private void HandleUserJoined(ClientPlayer player)
        {
            _Scene.CreatePlayer(player.Id, player.Id == LocalPlayer.Id);
            _Scene.HUD.ShowMessage(true, $"{player.Alias} has entered the game");
        }

        private void HandleUserLeft(ClientPlayer player)
        {
            _Scene.DestroyEntity(player.Id);
            _Scene.HUD.ShowMessage(true, $"{player.Alias} has left the game");
        }

        private void HandleShotFired(Shot shot)
        {
            _Scene.CreateProjectile(shot.Id, shot.SourceWeapon, shot.Position, shot.Direction, shot.Primary);
        }

        private void HandleShotRemoved(Shot shot)
        {
            _Scene.DestroyEntity(shot.Id);
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
            _Game.MenuController.Activate("Connection Lost");//$
        }
    }
}