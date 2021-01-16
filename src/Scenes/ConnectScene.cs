using System;
using SFML.System;
using BlackCoat;
using BlackCoat.Entities;

namespace BlackTournament.Scenes
{
    class ConnectScene:Scene
    {
        private TextItem _Text;
        private string _Host;

        public ConnectScene(Core core, String host) : base(core)
        {
            _Host = host;
        }

        protected override bool Load()
        {
            // Load Scene
            _Text = new TextItem(_Core) { Font = Game.DefaultFont };
            _Text.Position = new Vector2f(60, 60);
            _Text.Text = $"Connecting to \"{_Host}\"";
            Layer_Game.Add(_Text);
            return true;
        }

        protected override void Update(float deltaT)
        {
        }

        protected override void Destroy()
        {
            Layer_Game.Remove(_Text);
            _Text = null;
        }
    }
}