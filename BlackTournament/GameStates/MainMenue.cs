using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using BlackTournament.Entities;
using SFML.System;

namespace BlackTournament.GameStates
{
    class MainMenue:BaseGameState
    {
        private GameText _Text;
        public MainMenue(Core core):base(core)
        {

        }

        public override bool Load()
        {

            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(300, 100);
            _Text.Text = "MAIN MENU";
            Layer_Game.AddChild(_Text);

            return true;
        }

        public override void Update(float deltaT)
        {

        }

        public override void Destroy()
        {

        }
    }
}
