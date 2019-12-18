using System;
using System.Collections.Generic;
using System.Linq;

using SFML.System;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Properties;

namespace BlackTournament.Entities
{
    internal class PlayerLight : Container
    {
        private IEntity _Player;

        public PlayerLight(Core core, IEntity player, TextureLoader loader) : base(core)
        {
            _Player = player;

            // Aura
            var tex = loader.Load(nameof(Resources.Pointlight), Resources.Pointlight);
            Add(new Graphic(_Core, tex)
            {
                BlendMode = BlendMode.Add,
                Origin = tex.Size.ToVector2f() / 2,
                Scale = new Vector2f(0.8f, 0.8f),
                Alpha = 0.8f
            });

            // Torch
            Add(new Graphic(_Core, loader.Load(Files.Misc_TorchCone))
            {
                BlendMode = BlendMode.Add,
                Origin = new Vector2f(10, 205)
            });

        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
            if (_Player.Disposed)
            {
                // commit suicide
                Parent.Remove(this);
                Dispose();
            }
            else
            {
                Visible = _Player.Visible;
                Position = _Player.Position;
                Rotation = _Player.Rotation;
            }
        }
    }
}