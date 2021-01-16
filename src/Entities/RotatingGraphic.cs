using BlackCoat;
using BlackCoat.Entities;
using SFML.Graphics;

namespace BlackTournament.Entities
{
    class RotatingGraphic : Graphic
    {
        public float Speed { get; set; }

        public RotatingGraphic(Core core, Texture texture, float speed) : base(core, texture)
        {
            Speed = speed;
        }

        public override void Update(float deltaT)
        {
            Rotation = MathHelper.ValidateAngle(Rotation + Speed * deltaT);
        }
    }
}