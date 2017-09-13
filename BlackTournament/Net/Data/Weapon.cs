using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.Net.Data
{
    class Weapon
    {
        private bool _Primary;
        private bool _Triggered;

        // Working Values
        private float _PrimaryFireRate;
        private int _PrimaryAmmo;
        private float _SecundaryFireRate;
        private int _SecundaryAmmo;

        // Weapon Data
        public WeaponData PrimaryWeapon { get; }
        public WeaponData SecundaryWeapon { get; }



        public event Action<Boolean> ShotFired = p => { };


        public Weapon(WeaponData primaryWeapon, WeaponData secundaryWeapon)
        {
            // Store Weapon Data
            PrimaryWeapon = primaryWeapon;
            SecundaryWeapon = secundaryWeapon;

            // Init Munitions
            _PrimaryAmmo = PrimaryWeapon.Ammunition;
            _SecundaryAmmo = SecundaryWeapon.Ammunition;
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
                    if (_PrimaryFireRate <= 0 && _PrimaryAmmo > 0)
                    {
                        _PrimaryFireRate = PrimaryWeapon.FireRate;
                        _PrimaryAmmo--;
                        ShotFired.Invoke(_Primary);
                    }
                }
                else
                {
                    if (_SecundaryFireRate <= 0 && _SecundaryAmmo > 0)
                    {
                        _SecundaryFireRate = SecundaryWeapon.FireRate;
                        _SecundaryAmmo--;
                        ShotFired.Invoke(_Primary);
                    }
                }
            }
        }
    }
}