using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public class Weapon
    {

        public PickupType WeaponType { get; }
        public int PrimaryAmmo { get; protected set; }
        public int SecundaryAmmo { get; protected set; }
        public bool Empty => PrimaryAmmo + SecundaryAmmo == 0;


        public Weapon(PickupType weaponType)
        {
            WeaponType = weaponType;
        }

        public void Serialize(NetOutgoingMessage m)
        {
            m.Write(PrimaryAmmo);
            m.Write(SecundaryAmmo);
        }

        public void Deserialize(NetIncomingMessage m)
        {
            PrimaryAmmo = m.ReadInt32();
            SecundaryAmmo = m.ReadInt32();
        }
    }
}