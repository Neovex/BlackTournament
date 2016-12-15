﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.InputMapping
{
    enum GameAction
    {
        Confirm,
        Cancel,

        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,

        ShootPrimary,
        ShootSecundary,

        NextWeapon,
        PreviousWeapon,

        ShowStats
    }
}