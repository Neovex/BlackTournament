using System;
using BlackCoat;
using BlackCoat.InputMapping;
using SFML.Window;

namespace BlackTournament.Systems
{
    // This class initializes a SimpleInputMap with a default mapping or a mapping from a file or settings (incomplete)
    // TODO : consider adding to engine as a core system
    public class InputMapper
    {
        private SimpleInputMap<GameAction> _CurrentMapping;


        /// <summary>
        /// Gets the a default mapping that fits most use cases
        /// </summary>
        public SimpleInputMap<GameAction> DefaultMapping { get; private set; }


        /// <summary>
        /// Gets or sets the current input mapping.
        /// </summary>
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

        public Input Input { get; private set; }


        /// <summary>
        /// Occurs when a user input was mapped to a game-action based on the current input mapping.
        /// </summary>
        public event Action<GameAction, Boolean> Action = (a, b) => { };


        /// <summary>
        /// Initializes a new instance of the <see cref="InputMapper"/> class.
        /// </summary>
        public InputMapper(Input input)
        {
            Input = input;
            Log.Debug(nameof(InputMapper), "created");
            CreateDefaultInputMap(); // Fixme: load from file at some point
            CurrentMapping = DefaultMapping;
            // TODO: add option to modify a mapping (not urgent)
        }

        private void CreateDefaultInputMap()
        {
            DefaultMapping = new SimpleInputMap<GameAction>(Input, "Default");

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
            DefaultMapping.AddScrollMapping(ScrollDirection.Up, GameAction.NextWeapon);
            DefaultMapping.AddScrollMapping(ScrollDirection.Down, GameAction.PreviousWeapon);
        }

        private void HandleInput(GameAction action, Boolean activate)
        {
            Action.Invoke(action, activate);
        }
    }
}