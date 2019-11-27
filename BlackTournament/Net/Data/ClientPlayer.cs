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
        public String Alias { get; set; }
        public Boolean IsAlive => Health > 0;
        public Dictionary<PickupType, Weapon> Weapons { get; }

        public override PickupType CurrentWeaponType
        {
            get { return base.CurrentWeaponType; }
            protected set
            {
                if(CurrentWeaponType != value) Log.Debug(CurrentWeaponType); // TODO : clean when these strange assignments have been analyzed
                base.CurrentWeaponType = value;
            }
        }

        public ClientPlayer(int id) : base(id)
        {
            Weapons = new Dictionary<PickupType, Weapon>();
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            base.Deserialize(m);
            var ownedWeapons = m.ReadInt32();
            if (ownedWeapons < Weapons.Count) Weapons.Clear();
            for (int i = 0; i < ownedWeapons; i++)
            {
                var type = (PickupType)m.ReadInt32();
                if (!Weapons.ContainsKey(type)) Weapons[type] = new Weapon(type);
                Weapons[type].Deserialize(m);
            }
        }
    }
}