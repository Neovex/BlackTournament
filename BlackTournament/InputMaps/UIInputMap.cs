using System;
using System.Collections.Generic;
using System.Linq;
using BlackCoat;
using BlackCoat.UI;
using BlackCoat.InputMapping;

namespace BlackTournament.InputMaps
{
    internal class UIInputMap : UIInputMap<GameAction>
    {
        private static readonly Dictionary<GameAction, (UiOperation, float)> _GameAction_TO_UiOperation = 
                            new Dictionary<GameAction, (UiOperation, float)>()
                            {
                                { GameAction.Confirm, (UiOperation.Confirm, 0) },
                                { GameAction.Cancel, (UiOperation.Cancel, 0) },
                                { GameAction.MoveUp, (UiOperation.Move, Direction.UP) },
                                { GameAction.MoveDown, (UiOperation.Move, Direction.DOWN) },
                                { GameAction.MoveLeft, (UiOperation.Move, Direction.LEFT) },
                                { GameAction.MoveRight, (UiOperation.Move, Direction.RIGHT) },
                                { GameAction.ShootPrimary, (UiOperation.Confirm, 0) },
                                { GameAction.ShootSecundary, (UiOperation.Cancel, 0) },
                                { GameAction.NextWeapon, (UiOperation.Scroll, Direction.UP) },
                                { GameAction.PreviousWeapon, (UiOperation.Scroll, Direction.DOWN) },
                                { GameAction.ShowStats, (UiOperation.None, 0) }
                            };

        public UIInputMap(SimpleInputMap<GameAction> inputMap):base(inputMap, _GameAction_TO_UiOperation)
        {
            // There is no Text-handling in a Input Map so we activate in manually
            Input.TextEntered += RaiseTextEnteredEvent;
        }
    }
}