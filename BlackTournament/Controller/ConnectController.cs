using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Properties;

namespace BlackTournament.Controller
{
    public class ConnectController : ControllerBase
    {
        private BlackTournamentClient _Client;
        private ConnectState _State;

        //public event Action ConnectionEstablished = () => { }; //?


        public ConnectController(Game game) : base(game)
        {
        }

        public void Activate(BlackTournamentClient client,String host, Int32 port)
        {
            if (_Client != null || _State != null) throw new Exception("Invalid Controller State");
            _Client = client;

            // Build and switch to Connect State
            Activate(_State = new ConnectState(_Game.Core, host));

            // Connect to Host
            _Client.ChangeLevelReceived += LevelReady;
            _Client.ConnectionHasBeenLost += ConnectionFailed;
            _Client.Connect(host, port);
        }

        protected override void StateReady()
        {
            // TODO : feed state (aka view) with data here and only now
        }

        protected override void StateReleased()
        {
            _Client.ChangeLevelReceived -= LevelReady;
            _Client.ConnectionHasBeenLost -= ConnectionFailed;
            _Client = null;
            _State = null;
        }

        private void LevelReady()
        {
            Log.Debug("Client Ready");
            _Game.Core.AnimationManager.Wait(2, a => { _Game.MapController.Activate(_Client); }); // TODO: move timeout to map controller?
        }

        private void ConnectionFailed()
        {
            _Game.MenuController.Activate("Connection Failed");//$
        }
    }
}