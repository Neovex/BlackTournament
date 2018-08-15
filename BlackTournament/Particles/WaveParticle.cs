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
    class WaveParticle : TextureParticle
    {
        private float _StartAlpha;
        private float _Endscale;
        private float _Speed;

        public float Scale { get; set; }

        public WaveParticle(Core core) : base(core)
        {
        }

        public void Initialize(Texture texture, Vector2f position, Vector2f origin, float startScale, float endScale, Color color, Single alpha, float speed)
        {
            _Texture = texture;
            _TextureRect.Width = (int)_Texture.Size.X;
            _TextureRect.Height = (int)_Texture.Size.Y;
            _Position = position;
            _Origin = origin;
            _Scale = new Vector2f(startScale, startScale);
            _Endscale = endScale;
            _Color = color;
            _Alpha = _StartAlpha = alpha;
            _Speed = speed;
        }

        protected override unsafe bool UpdateInternal(float deltaT, Vertex* vPtr)
        {
            _Scale.X = _Scale.Y += _Speed * deltaT;
            _Alpha = _StartAlpha * (1 - (_Scale.X / _Endscale));
            return base.UpdateInternal(deltaT, vPtr) || _Alpha <= 0;
        }
    }
}