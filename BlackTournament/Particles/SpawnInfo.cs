using System;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.Graphics;
using SFML.System;

namespace BlackTournament.Particles
{
    class SpawnInfo : TextureParticleInitializationInfo
    {
        private readonly Core _Core;

        public override Vector2f Offset { get => _Core.Random.NextVector(-7, 7); set => base.Offset = value; }
        public override Vector2f Scale { get => base.Scale * _Core.Random.NextFloat(0.8f, 1); set => base.Scale = value; }
        public override float Rotation { get => _Core.Random.NextFloat(0, 360); set => base.Rotation = value; }
        public override Vector2f Velocity { get => base.Velocity + _Core.Random.NextVector(-10, 10, -5, 10); set => base.Velocity = value; }
        public override float AlphaFade { get => base.AlphaFade + _Core.Random.NextFloat(-1.5f, -0.5f); set => base.AlphaFade = value; }

        public SpawnInfo(Core core, Texture texture, Net.Data.PickupType type) : base(texture)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            switch (type)
            {
                case Net.Data.PickupType.Drake:
                case Net.Data.PickupType.SmallHealth:
                case Net.Data.PickupType.BigHealth:
                    Color = Color.Magenta;
                    break;
                case Net.Data.PickupType.Hedgeshock:
                case Net.Data.PickupType.SmallShield:
                case Net.Data.PickupType.BigShield:
                    Color = Color.Cyan;
                    break;
                case Net.Data.PickupType.Thumper:
                    Color = Color.Green;
                    break;
                case Net.Data.PickupType.Titandrill:
                    Color = Color.Yellow;
                    break;
            }

            Scale = new Vector2f(0.01f, 0.01f);
            Origin = texture.Size.ToVector2f() / 2;

            Velocity = new Vector2f(0, -35);

            Loop = true;
            SpawnRate = 0.1f;
            TTL = 10;
            UseAlphaAsTTL = true;
        }
    }
}
