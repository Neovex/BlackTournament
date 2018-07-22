using System;
using System.Linq;
using System.Collections.Generic;

using SFML.Graphics;
using SFML.System;

using BlackCoat.Entities;
using BlackCoat;

namespace BlackTournament.Entities
{
    class MapRenderer : BaseEntity
    {
        private Vertex[] _Vertices;
        private Texture _Texture;
        private Vector2i _TileSize;


        public override Color Color { get; set; }


        public MapRenderer(Core core, Vector2i mapSize, Texture texture, Vector2i tileSize) : base(core)
        {
            if (texture == null || mapSize.X < 1 || mapSize.Y < 1 || tileSize.X < 1 || tileSize.Y < 1) throw new ArgumentException();

            _Vertices = new Vertex[mapSize.X * mapSize.Y * 4];
            _Texture = texture;
            _TileSize = tileSize;
        }


        public void AddTile(int index, Vector2f pos, Vector2i tex)
        {
            var c = Color;
            if (tex.X < 0) c.A = 0;

            var v = new Vertex();
            v.Position.X = pos.X;
            v.Position.Y = pos.Y;
            v.TexCoords.X = tex.X;
            v.TexCoords.Y = tex.Y;
            v.Color = c;
            _Vertices[index] = v;


            v.Position.X = pos.X + _TileSize.X;
            v.Position.Y = pos.Y;
            v.TexCoords.X = tex.X + _TileSize.X;
            v.TexCoords.Y = tex.Y;
            v.Color = c;
            _Vertices[index + 1] = v;
            

            v.Position.X = pos.X + _TileSize.X;
            v.Position.Y = pos.Y + _TileSize.Y;
            v.TexCoords.X = tex.X + _TileSize.X;
            v.TexCoords.Y = tex.Y + _TileSize.Y;
            v.Color = c;
            _Vertices[index + 2] = v;
            

            v.Position.X = pos.X;
            v.Position.Y = pos.Y + _TileSize.Y;
            v.TexCoords.X = tex.X;
            v.TexCoords.Y = tex.Y + _TileSize.Y;
            v.Color = c;
            _Vertices[index + 3] = v;
        }

        public override void Update(float deltaT)
        {
            // nothing to do here
        }

        public override void Draw()
        {
            _Core.Draw(this);
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            states.Texture = _Texture;
            target.Draw(_Vertices, PrimitiveType.Quads, states);
        }
    }
}