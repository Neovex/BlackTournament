using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public class GameInfo : NetEntityBase
    {
        public override EntityType EntityType { get { return EntityType.Game; } }

        public String ServerName { get; set; }
        public String GameType { get; set; }


        public GameInfo() : base(0)
        {
        }
        public GameInfo(NetIncomingMessage m) : base(0, m)
        {
        }


        public override void Deserialize(NetIncomingMessage m)
        {
            ServerName = m.ReadString();
            GameType = m.ReadString();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(ServerName);
            m.Write(GameType);
        }
    }
}