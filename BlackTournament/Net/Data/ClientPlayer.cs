using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public class ClientPlayer:Player
    {
        public String Alias { get; set; }

        public ClientPlayer(NetIncomingMessage m) : base(m)
        {
        }

        public ClientPlayer(int id):base(id)
        {

        }
    }
}
