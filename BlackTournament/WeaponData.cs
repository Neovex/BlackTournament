using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat.Collision;
using BlackTournament.Net.Data;

namespace BlackTournament
{
    class WeaponData
    {
        private const int _INF = 100000;

        public static readonly WeaponData DrakePrimary = new WeaponData(14.3f, 0, 200, 0.075f, _INF, 0, 0, Geometry.Line, 4);
        public static readonly WeaponData DrakeSecundary = new WeaponData(35, 80, 2, 0.7f, 0, 700, 1.5f, Geometry.Point);

        public static readonly WeaponData HedgeshockPrimary = new WeaponData(10, 0, 180, 0.05f, 200, 0, 0, Geometry.Line, 15);
        public static readonly WeaponData HedgeshockSecundary = new WeaponData(1, 20, 6, 0.3f, 0, 600, 3, Geometry.Circle); // check penetration damage

        public static readonly WeaponData ThumperPrimary = new WeaponData(DrakeSecundary.Damage, DrakeSecundary.BlastRadius, 10, 0.6f, DrakeSecundary.Length, DrakeSecundary.Speed, DrakeSecundary.TTL, DrakeSecundary.ProjectileGeometry);
        public static readonly WeaponData ThumperSecundary = new WeaponData(DrakeSecundary.Damage*0.8f, DrakeSecundary.BlastRadius*0.8f, 4, 0.9f, DrakeSecundary.Length, DrakeSecundary.Speed * 1.2f, DrakeSecundary.TTL, DrakeSecundary.ProjectileGeometry);

        public static readonly WeaponData TitandrillPrimary = new WeaponData(25, 0, 30, 0.8f, 500, 0, 0, Geometry.Line);
        public static readonly WeaponData TitandrillSecundary = new WeaponData(99, 0, 2, 2, _INF, 0, 0, Geometry.Line);


        public static Weapon CreateWeaponFrom(PickupType pickup)
        {
            switch (pickup)
            {
                case PickupType.Drake:      return new Weapon(DrakePrimary, DrakeSecundary);
                case PickupType.Hedgeshock: return new Weapon(HedgeshockPrimary, HedgeshockSecundary);
                case PickupType.Thumper:    return new Weapon(ThumperPrimary, ThumperSecundary);
                case PickupType.Titandrill: return new Weapon(TitandrillPrimary, TitandrillSecundary);
            }
            var msg = "Invalid weapon request";
            Log.Fatal(msg);
            throw new Exception(msg);
        }



        // Weapon Stats
        public float Damage { get; }
        public float BlastRadius { get; }
        public int Ammunition { get; }
        public float FireRate { get; }

        // Projectile Stats
        public float Length { get; }
        public float Speed { get; }
        public float TTL { get; }
        public Geometry ProjectileGeometry { get; }
        public float Inaccuracy { get; }

        private WeaponData(float damage, float blastRadius, int ammunition, float fireRate, float length, float speed, float ttl, Geometry projectileGeometry, float inaccuracy = 0)
        {
            Damage = damage;
            BlastRadius = blastRadius;
            Ammunition = ammunition;
            FireRate = fireRate;
            Length = length;
            Speed = speed;
            TTL = ttl;
            ProjectileGeometry = projectileGeometry;
            Inaccuracy = inaccuracy;
        }
    }
}