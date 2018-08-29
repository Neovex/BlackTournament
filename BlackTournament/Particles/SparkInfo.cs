using System;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;
using BlackTournament.Net.Data;

namespace BlackTournament.Particles
{
    class SparkInfo : PixelParticleInitializationInfo
    {
        private readonly Core _Core;
        private readonly float _Speed;
        private readonly float _Spread;

        public override Vector2f Velocity
        {
            set => base.Velocity = value;
            get
            {
                var dmin = (Direction ?? _Spread) - _Spread;
                var dmax = (Direction ?? 360 - _Spread) + _Spread;
                return Create.Vector2fFromAngleLookup(_Core.Random.NextFloat(dmin, dmax), _Speed + _Core.Random.NextFloat(-80, 60));
            }
        }
        //public override bool UseAlphaAsTTL { get => true; set => base.UseAlphaAsTTL = value; }
        public override float TTL { get => base.TTL + _Core.Random.NextFloat(-0.05f, 0.05f); set => base.TTL = value; }
        public float? Direction { get; set; }
        public Color DefaultColor { get; internal set; }

        public SparkInfo(Core core, float speed, float spread, float? direction = null)
        {
            _Core = core;
            _Speed = speed;
            _Spread = spread;
            Direction = direction;
        }

        public void Update(PickupType weapon)
        {
            switch (weapon)
            {
                case PickupType.Drake:
                case PickupType.Thumper:
                    ParticlesPerSpawn = 6;
                    Color = DefaultColor;
                    break;
                case PickupType.Hedgeshock:
                    ParticlesPerSpawn = 4;
                    Color = Color.Cyan;
                    break;
                case PickupType.Titandrill:
                    ParticlesPerSpawn = 22;
                    Color = DefaultColor;
                    break;
            }
        }
    }
}