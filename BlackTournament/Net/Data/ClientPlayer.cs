using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    public class ClientPlayer : Player // TODO : remove unnecessary overrides when base game is stable
    {
        public String Alias { get; set; }
        public Boolean IsAlive => Health > 0;

        public event Action<ClientPlayer> Fragged = p => { };

        public override Vector2f Position
        {
            get { return base.Position; }
            protected set { base.Position = value; }
        }

        public override float Rotation
        {
            get { return base.Rotation; }
            protected set { base.Rotation = value; }
        }

        public override int Health
        {
            get { return base.Health; }
            protected set
            {
                base.Health = value;
                if (Health < 1) Fragged.Invoke(this);
            }
        }

        public override int Shield
        {
            get { return base.Shield; }
            protected set { base.Shield = value; }
        }

        public override int Score
        {
            get { return base.Score; }
            protected set { base.Score = value; }
        }

        public ClientPlayer(int id) : base(id)
        {
        }

        public ClientPlayer(int id, NetIncomingMessage m) : base(id, m)
        {
        }
    }
}