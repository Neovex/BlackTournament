using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.System;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public class LGameClient:LClient<GameMessageType>
    {
        private NetClient _Client;
        

        public override int AdminId { get { return Net.ADMIN_ID; } }
        public string MapName { get; private set; }

        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionHasBeenLost = () => { };
        
        public event Action<int, float, float, float> UpdatePositionReceived = (id, x, y, a) => { };
        public event Action<int> ShotReceived = id => { };
        public event Action<string> ChangeLevelReceived = lvl => { };



        public LGameClient(String userName):base(userName, Net.COMMANDS)
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
    }
}