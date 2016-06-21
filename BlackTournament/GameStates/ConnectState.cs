using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using BlackTournament.Entities;
using SFML.System;
using BlackTournament.Net;

namespace BlackTournament.GameStates
{
    class ConnectState:BaseGameState
    {
        private GameText _Text;
        private NetworkManager _NetworkManager;
        private string _Host;
        private uint _Port;

        public ConnectState(Core core, NetworkManager netMan, string host, uint port) : base(core)
        {
            _NetworkManager = netMan;
            _Host = host;
            _Port = port;
        }

        public override bool Load() // CSH
        {
            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(30, 30);
            _Text.Text = "Connecting to" + _Host;
            Layer_Game.AddChild(_Text);

            _NetworkManager.Connected += Connected;
            _NetworkManager.Connect(_Host, _Port);

            return true;
        }

        private void Connected()
        {
            _Core.StateManager.ChangeState(new MapState(_Core, _NetworkManager));
        }

        public override void Update(float deltaT)
        {
            // add some fancy waiting animation here
        }

        public override void Destroy()
        {
            _NetworkManager.Connected -= Connected;
        }
    }
}