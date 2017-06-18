using System;
using BlackCoat.InputMapping;
using SFML.Window;

namespace BlackTournament.Systems
{
    // TODO : consider adding to engine as core system
    public class InputMapper
    {
        private SimpleInputMap<GameAction> _CurrentMapping;


        /// <summary>
        /// Gets the a default mapping that fits most use cases
        /// </summary>
        public SimpleInputMap<GameAction> DefaultMapping { get; private set; }

        /// <summary>
        /// Gets and sets the exclusive listener.
        /// WARNING: As long as the ExclusiveListener is not null the Action Event will not be raised!
        /// </summary>
        public Action<GameAction, Boolean> ExclusiveListener { get; private set; }


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


        /// <summary>
        /// Occurs when a user input was mapped to a game-action based on the current input mapping.
        /// WARNING: As long as the ExclusiveListener is not null the Action Event will not be raised!
        /// </summary>
        public event Action<GameAction, Boolean> Action = (a, b) => { };


        /// <summary>
        /// Initializes a new instance of the <see cref="InputMapper"/> class.
        /// </summary>
        public InputMapper()
        {
            Log.Debug(nameof(InputMapper), "created");
            CreateDefaultInputMap(); // Fixme: load from file at some point
            CurrentMapping = DefaultMapping;
            // TODO: add option to modify a mapping (not urgent)
        }

        private void CreateDefaultInputMap()
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
            DefaultMapping.AddScrollMapping(ScrollDirection.Up, GameAction.NextWeapon);
            DefaultMapping.AddScrollMapping(ScrollDirection.Down, GameAction.PreviousWeapon);
        }

        private void HandleInput(GameAction action, Boolean activate)
        {
            (ExclusiveListener ?? Action).Invoke(action, activate);
        }
    }
}