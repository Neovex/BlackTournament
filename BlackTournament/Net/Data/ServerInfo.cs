using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public class ServerInfo:NetEntityBase
    {
        public String Name { get; private set; }
        public String Map { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; private set; }
        public int Ping { get; set; }
        public Boolean HasPassword { get; set; }

        public ServerInfo(string serverName, string map, int maxPlayers = 8) : base(0)
        {
            Name = serverName;
            Map = map;
            MaxPlayers = maxPlayers;
        }
        public ServerInfo(NetIncomingMessage msg):base(0, msg)
        {
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Name);
            m.Write(Map);
            m.Write(CurrentPlayers);
            m.Write(MaxPlayers);
            m.Write(HasPassword);
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            m.ReadInt32(); // skip id
            Name = m.ReadString();
            Map = m.ReadString();
            CurrentPlayers = m.ReadInt32();
            MaxPlayers = m.ReadInt32();
            HasPassword = m.ReadBoolean();
        }
    }
}