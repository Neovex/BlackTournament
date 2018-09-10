using System;
using System.Linq;

using SFML.System;

using BlackCoat;
using BlackTournament.Entities;


namespace BlackTournament.GameStates
{
    class ConnectState:Gamestate
    {
        private GameText _Text;
        private string _Host;

        public ConnectState(Core core, String host) : base(core)
        {
            _Host = host;
        }

        protected override bool Load()
        {
            // Load Scene
            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(60, 60);
            _Text.Text = $"Connecting to \"{_Host}\"";
            return Layer_Game.Add(_Text);
        }

        protected override void Update(float deltaT)
        {
            // todo: add some fancy waiting animation here
        }

        protected override void Destroy()
        {
            Layer_Game.Remove(_Text);
        }
    }
}