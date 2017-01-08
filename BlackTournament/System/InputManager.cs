using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat.InputMapping;
using SFML.Window;

namespace BlackTournament.System
{
    public class InputManager
    {
        public SimpleInputMap<GameAction> DefaultMapping { get; private set; }
        private SimpleInputMap<GameAction> _CurrentMapping;

        public SimpleInputMap<GameAction> CurrentMapping
        {
            get { return _CurrentMapping; }
            set
            {
                if (_CurrentMapping != null) _CurrentMapping.MappedOperationInvoked -= HandleInput;
                Log.Debug("Set to", value?.Name);
                _CurrentMapping = value;
                if (_CurrentMapping != null) _CurrentMapping.MappedOperationInvoked += HandleInput;
            }
        }

        public event Action<GameAction> Action = a => { };


        public InputManager()
        {
            Log.Debug(nameof(InputManager), "created");
            CreateDefaultMap(); // Fixme: load from file at some point
            CurrentMapping = DefaultMapping;
            // TODO: add option to modify a mapping (not urgent)
        }

        private void CreateDefaultMap()
        {
            DefaultMapping = new SimpleInputMap<GameAction>("Default");

            // Movement
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.W, GameAction.MoveUp);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.Up, GameAction.MoveUp);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.A, GameAction.MoveLeft);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.Left, GameAction.MoveLeft);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.S, GameAction.MoveDown);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.Down, GameAction.MoveDown);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.D, GameAction.MoveRight);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.Right, GameAction.MoveRight);

            // UI
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.Return, GameAction.Confirm);
            DefaultMapping.AddKeyboardMapping(Keyboard.Key.Escape, GameAction.Cancel);

            // Mouse
            DefaultMapping.AddMouseMapping(Mouse.Button.Left, GameAction.ShootPrimary);
            DefaultMapping.AddMouseMapping(Mouse.Button.Right, GameAction.ShootSecundary);
            DefaultMapping.AddMouseMapping(Mouse.Button.XButton1, GameAction.NextWeapon);
            DefaultMapping.AddMouseMapping(Mouse.Button.XButton2, GameAction.PreviousWeapon);

            // Scroll
            DefaultMapping.AddScrollMapping(1f, GameAction.NextWeapon);
            DefaultMapping.AddScrollMapping(-1f, GameAction.PreviousWeapon);
        }

        private void HandleInput(GameAction action)
        {
            //Log.Debug(action);
            Action.Invoke(action);
        }
    }
}