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

        public override float Health
        {
            get { return base.Health; }
            protected set
            {
                base.Health = value;
                if (Health < 1) Fragged.Invoke(this);
            }
        }

        public override PickupType CurrentWeaponType
        {
            get { return base.CurrentWeaponType; }
            protected set
            {
                if(CurrentWeaponType != value) Log.Debug(CurrentWeaponType);
                base.CurrentWeaponType = value;
            }
        }

        public ClientPlayer(int id) : base(id)
        {
        }

        public ClientPlayer(int id, NetIncomingMessage m) : base(id, m)
        {
        }
    }
}