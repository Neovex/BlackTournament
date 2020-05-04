using System;
using System.Net;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public class ServerInfo:NetEntityBase
    {
        public IPEndPoint EndPoint { get; }

        public String Name { get; private set; }
        public String Map { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; private set; }
        public int Ping { get; set; }
        //public Boolean HasPassword { get; set; }


        public ServerInfo(string serverName, string map, int maxPlayers = 8) : base(0)
        {
            Name = serverName;
            Map = map;
            MaxPlayers = maxPlayers;
        }
        public ServerInfo(IPEndPoint endPoint, NetIncomingMessage msg):base(0, msg)
        {
            EndPoint = endPoint;
        }

        protected override void SerializeInternal(NetOutgoingMessage m, bool fullSync)
        {
            m.Write(Name);
            m.Write(Map);
            m.Write(CurrentPlayers);
            m.Write(MaxPlayers);
            m.Write(Ping);
            //m.Write(HasPassword);
        }

        protected override void DeserializeInternal(NetIncomingMessage m, bool fullSync)
        {

            Name = m.ReadString();
            Map = m.ReadString();
            CurrentPlayers = m.ReadInt32();
            MaxPlayers = m.ReadInt32();
            Ping = m.ReadInt32();
            //HasPassword = m.ReadBoolean();
        }
    }
}