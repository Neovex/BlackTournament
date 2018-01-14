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

        public static readonly WeaponData DrakePrimary = new WeaponData(14.3f, 200, 0.075f, _INF, 0, 0, Geometry.Line);
        public static readonly WeaponData DrakeSecundary = new WeaponData(35, 2, 0.7f, 0, 500, 2, Geometry.Point);

        public static readonly WeaponData HedgeshockPrimary = new WeaponData(10, 180, 0.0666f, 280, 0, 0, Geometry.Line);
        public static readonly WeaponData HedgeshockSecundary = new WeaponData(1, 6, 0.3f, 0, 300, 3, Geometry.Circle); // check penetration damage

        public static readonly WeaponData ThumperPrimary = new WeaponData(DrakeSecundary.Damage, 10, 0.5f, 0, DrakeSecundary.Speed, DrakeSecundary.TTL, DrakeSecundary.ProjectileGeometry);
        public static readonly WeaponData ThumperSecundary = new WeaponData(0, 100, 0.5f, 70, 0, 0, Geometry.Circle); // Fix ttl for thumper

        public static readonly WeaponData TitandrillPrimary = new WeaponData(8, 30, 0.2f, 500, 0, 0, Geometry.Line);
        public static readonly WeaponData TitandrillSecundary = new WeaponData(99, 1, 1, _INF, 0, 0, Geometry.Line);


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
        public int Ammunition { get; }
        public float FireRate { get; }

        // Projectile Stats
        public float Length { get; }
        public float Speed { get; }
        public float TTL { get; }
        public Geometry ProjectileGeometry { get; }


        private WeaponData(float damage, int ammunition, float fireRate, float length, float speed, float ttl, Geometry projectileGeometry)
        {
            Damage = damage;
            Ammunition = ammunition;
            FireRate = fireRate;
            Length = length;
            Speed = speed;
            TTL = ttl;
            ProjectileGeometry = projectileGeometry;
        }
    }
}