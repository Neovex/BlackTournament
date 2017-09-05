﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    public class Impact : NetEntityBase
    {
        public Vector2f Position { get; private set; }
        public PickupType Source { get; private set; }

        public Impact(int id, Vector2f position, PickupType source) : base(id)
        {
            Position = position;
            Source = source;
        }

        public Impact(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Source = (PickupType)m.ReadInt32();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write((int)Source);
        }
    }
}