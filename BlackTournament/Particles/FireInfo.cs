using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.Graphics;
using SFML.System;

namespace BlackTournament.Particles
{
    class FireInfo : TexturedSpawnInfo
    {
        private readonly Core _Core;
        private readonly float _Speed;

        public override Vector2f Offset { get => _Core.Random.NextVector(-7, 7); set => base.Offset = value; }
        public override Vector2f Scale { get => base.Scale * _Core.Random.NextFloat(0.8f, 1); set => base.Scale = value; }
        public override float Rotation { get => _Core.Random.NextFloat(0, 360); set => base.Rotation = value; }
        public override Vector2f Velocity { get => base.Velocity + Create.Vector2fFromAngleLookup(_Core.Random.NextFloat(0, 360), _Speed); set => base.Velocity = value; }
        public override float RotationVelocity { get => base.RotationVelocity + _Core.Random.NextFloat(-_Speed, _Speed); set => base.RotationVelocity = value; }
        public override float AlphaFade { get => base.AlphaFade + _Core.Random.NextFloat(-1.5f, -0.5f); set => base.AlphaFade = value; }

        public FireInfo(Core core, Texture texture, float speed) : base(texture)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            _Speed = speed;

            Color = new Color(255, 100, 20, 255);
            Scale = new Vector2f(0.2f, 0.2f);
            Origin = texture.Size.ToVector2f() / 2;

            Velocity = new Vector2f(0, -30);

            Loop = true;
            SpawnRate = 0.03f;
            TTL = 10;
            UseAlphaAsTTL = true;
        }
    }
}
