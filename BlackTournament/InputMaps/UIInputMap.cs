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
        private static readonly Dictionary<GameAction, (UiOperation, UiOperation, float)> _GameAction_TO_UiOperation = 
                            new Dictionary<GameAction, (UiOperation, UiOperation, float)>()
                            {
                                { GameAction.Confirm, (UiOperation.BeforeConfirm, UiOperation.Confirm, 0) },
                                { GameAction.Cancel, (UiOperation.Cancel, UiOperation.None, 0) },
                                { GameAction.MoveUp, (UiOperation.Move, UiOperation.None, Direction.UP) },
                                { GameAction.MoveDown, (UiOperation.Move, UiOperation.None, Direction.DOWN) },
                                { GameAction.MoveLeft, (UiOperation.Move, UiOperation.None, Direction.LEFT) },
                                { GameAction.MoveRight, (UiOperation.Move, UiOperation.None, Direction.RIGHT) },
                                { GameAction.ShootPrimary, (UiOperation.BeforeConfirm, UiOperation.Confirm, 0) },
                                { GameAction.ShootSecundary, (UiOperation.Cancel, UiOperation.None, 0) },
                                { GameAction.NextWeapon, (UiOperation.Scroll, UiOperation.None, Direction.UP) },
                                { GameAction.PreviousWeapon, (UiOperation.Scroll, UiOperation.None, Direction.DOWN) },
                                { GameAction.ShowStats, (UiOperation.None, UiOperation.None, 0) }
                            };

        public UIInputMap(SimpleInputMap<GameAction> inputMap):base(inputMap, _GameAction_TO_UiOperation)
        {
            // There is no Text-handling in a Input Map so we activate in manually
            Input.TextEntered += RaiseTextEnteredEvent;
        }
    }
}