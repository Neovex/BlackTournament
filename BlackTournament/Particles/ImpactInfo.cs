using BlackCoat;
using BlackCoat.ParticleSystem;
using BlackTournament.Net.Data;
using SFML.System;
using SFML.Graphics;

namespace BlackTournament.Particles
{
    class ImpactInfo:PixelParticleInitializationInfo
    {
        private Core _Core;
        private float _Spread;

        public override Vector2f Offset { get => VectorExtensions.VectorFromAngleLookup(_Core.Random.NextFloat(0, 360), _Core.Random.NextFloat(0, _Spread)); set => base.Offset = value; }

        public ImpactInfo(Core core)
        {
            _Core = core;
        }

        public void Update(PickupType weapon)
        {
            switch (weapon)
            {
                case PickupType.Drake:
                    _Spread = 1;
                    Color = Color.Yellow;
                    ParticlesPerSpawn = 6;
                    break;
                case PickupType.Hedgeshock:
                    _Spread = 1;
                    ParticlesPerSpawn = 4;
                    Color = Color.Cyan;
                    break;
                case PickupType.Titandrill:
                    _Spread = 2.5f;
                    Color = Color.Yellow;
                    ParticlesPerSpawn = 30;
                    break;
            }
        }
    }
}