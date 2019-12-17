using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Properties;

namespace BlackTournament.Entities
{
    internal class OrbLight : Graphic
    {
        private IEntity _Target;

        public OrbLight(Core core, IEntity target, TextureLoader loader) : base(core)
        {
            _Target = target;

            // Light
            Texture = loader.Load(nameof(Resources.Pointlight), Resources.Pointlight);
            BlendMode = BlendMode.Add;
            Origin = Texture.Size.ToVector2f() / 2;
            Scale = (0.8f, 0.8f);
            Color = new Color(200, 200, 255);
        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
            if (_Target.Disposed)
            {
                // commit suicide
                Parent.Remove(this);
                Dispose();
            }
            else
            {
                Position = _Target.Position;
            }
        }
    }
}