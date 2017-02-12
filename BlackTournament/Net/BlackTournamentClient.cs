using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Net.Client;
using BlackTournament.Systems;
using Lidgren.Network;

namespace BlackTournament.Net
{
    public class BlackTournamentClient : ManagedClient<GameMessageType>
    {
        public override int AdminId { get { return Net.ADMIN_ID; } }
        public string MapName { get; private set; }
        public Boolean IsConnected { get { throw new NotImplementedException(); } }

        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionHasBeenLost = () => { };
        
        public event Action<int, float, float, float> UpdatePositionReceived = (id, x, y, a) => { };
        public event Action<int> ShotReceived = id => { };
        public event Action<string> ChangeLevelReceived = lvl => { };



        public BlackTournamentClient(String userName):base(userName, Net.COMMANDS)
        {
        }

        protected override void DataReceived(GameMessageType subType, NetIncomingMessage msg)
        {
            throw new NotImplementedException();
        }



        protected override void Connected(int id, string alias)
        {
            ConnectionEstablished.Invoke();
        }

        internal void ProcessGameAction(GameAction obj)
        {
            throw new NotImplementedException();
        }

        protected override void Disconnected()
        {
            ConnectionHasBeenLost.Invoke();
        }



        protected override void UserConnected(ClientUser user)
        {
        }

        protected override void UserDisconnected(ClientUser user)
        {
        }


        public void SendMessage(string txt)
        {
            Send(GameMessageType.Message, m => m.Write(txt));
        }

        internal void StopServer()
        {
            throw new NotImplementedException();
        }

        internal void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}