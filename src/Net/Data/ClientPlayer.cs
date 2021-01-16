using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    public class ClientPlayer : Player
    {
        public String Alias { get; }

        public ClientPlayer(int id, string alias)
        {
            Id = id;
            Alias = alias;
        }
    }
}