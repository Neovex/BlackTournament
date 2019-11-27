using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.Net.Data
{
    class ServerWeapon : Weapon
    {
        private bool _Primary;
        private bool _Triggered;

        private float _PrimaryFireRate;
        private float _SecundaryFireRate;


        public event Action<Boolean> ShotFired = p => { };


        public ServerWeapon(PickupType weaponType) : base(weaponType)
        {
            var primary = WeaponData.Get(WeaponType, true);
            var secundary = WeaponData.Get(WeaponType, false);

            _PrimaryFireRate = primary.FireRate;
            _SecundaryFireRate = secundary.FireRate;
            PrimaryAmmo = primary.Ammunition;
            SecundaryAmmo = secundary.Ammunition;
        }

        public void Fire(bool primary)
        {
            if (_Triggered) Release();

            _Primary = primary;
            _Triggered = true;
        }
        public void Release()
        {
            _Triggered = false;
        }

        public void Update(float deltaT)
        {
            _PrimaryFireRate -= deltaT;
            _SecundaryFireRate -= deltaT;

            if (_Triggered)
            {
                if (_Primary)
                {
                    if (_PrimaryFireRate <= 0 && PrimaryAmmo > 0)
                    {
                        _PrimaryFireRate = WeaponData.Get(WeaponType, true).FireRate;
                        PrimaryAmmo--;
                        ShotFired.Invoke(_Primary);
                    }
                }
                else
                {
                    if (_SecundaryFireRate <= 0 && SecundaryAmmo > 0)
                    {
                        _SecundaryFireRate = WeaponData.Get(WeaponType, false).FireRate;
                        SecundaryAmmo--;
                        ShotFired.Invoke(_Primary);
                    }
                }
            }
        }
    }
}