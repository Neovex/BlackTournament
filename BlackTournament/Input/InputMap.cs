using System;
using SFML.Window;
using BlackCoat.InputMapping;

namespace BlackTournament.Input
{
    // This class initializes a SimpleInputMap with a default mapping or a mapping from a file or settings (incomplete)
    public class InputMap : SimpleInputMap<GameAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputMap"/> class.
        /// </summary>
        public InputMap(BlackCoat.Input input) : base(input, nameof(InputMap))
        {
            Log.Debug(Name, "created");
            CreateDefaultInputMap(); // Fixme: load from file at some point
        }

        private void CreateDefaultInputMap()
        {
            // Movement
            AddKeyboardMapping(Keyboard.Key.W, GameAction.MoveUp);
            AddKeyboardMapping(Keyboard.Key.Up, GameAction.MoveUp);
            AddKeyboardMapping(Keyboard.Key.A, GameAction.MoveLeft);
            AddKeyboardMapping(Keyboard.Key.Left, GameAction.MoveLeft);
            AddKeyboardMapping(Keyboard.Key.S, GameAction.MoveDown);
            AddKeyboardMapping(Keyboard.Key.Down, GameAction.MoveDown);
            AddKeyboardMapping(Keyboard.Key.D, GameAction.MoveRight);
            AddKeyboardMapping(Keyboard.Key.Right, GameAction.MoveRight);

            // UI
            AddKeyboardMapping(Keyboard.Key.Return, GameAction.Confirm);
            AddKeyboardMapping(Keyboard.Key.Escape, GameAction.Cancel);

            // Mouse
            AddMouseMapping(Mouse.Button.Left, GameAction.ShootPrimary);
            AddMouseMapping(Mouse.Button.Right, GameAction.ShootSecundary);
            AddMouseMapping(Mouse.Button.XButton1, GameAction.NextWeapon);
            AddMouseMapping(Mouse.Button.XButton2, GameAction.PreviousWeapon);

            // Scroll
            AddScrollMapping(ScrollDirection.Up, GameAction.NextWeapon);
            AddScrollMapping(ScrollDirection.Down, GameAction.PreviousWeapon);
        }
    }
}