using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using BlackCoat.Entities;
using BlackCoat;

namespace BlackTournament
{
    class MapRenderer : BaseEntity
    {
        private Vertex[] _Vertices;
        private Texture _Texture;
        private Vector2i _TileSize;


        public override Color Color { get; set; }


        public MapRenderer(Core core, Vector2i mapSize, Texture texture, Vector2i tileSize):base(core)
        {
            if (texture == null || mapSize.X < 1 || mapSize.Y < 1 || tileSize.X < 1 || tileSize.Y < 1) throw new ArgumentException();

            _Vertices = new Vertex[mapSize.X * mapSize.Y * 4];
            _Texture = texture;
            _TileSize = tileSize;

            Visible = true;
            Alpha = 1;
            Color = Color.White;
        }

        public void AddTile(int index, Vector2f pos, Vector2i tex)
        {
            var v = new Vertex();
            v.Position.X = pos.X;
            v.Position.Y = pos.Y;
            v.TexCoords.X = tex.X;
            v.TexCoords.Y = tex.Y;
            v.Color = Color;
            _Vertices[index] = v;


            v.Position.X = pos.X + _TileSize.X;
            v.Position.Y = pos.Y;
            v.TexCoords.X = tex.X + _TileSize.X;
            v.TexCoords.Y = tex.Y;
            v.Color = Color;
            _Vertices[index + 1] = v;
            

            v.Position.X = pos.X + _TileSize.X;
            v.Position.Y = pos.Y + _TileSize.Y;
            v.TexCoords.X = tex.X + _TileSize.X;
            v.TexCoords.Y = tex.Y + _TileSize.Y;
            v.Color = Color;
            _Vertices[index + 2] = v;
            

            v.Position.X = pos.X;
            v.Position.Y = pos.Y + _TileSize.Y;
            v.TexCoords.X = tex.X;
            v.TexCoords.Y = tex.Y + _TileSize.Y;
            v.Color = Color;
            _Vertices[index + 3] = v;
        }

        /*
                public void Refresh()
                {
                    RefreshLocal(0, 0, _Width, _Height);
                }

                private void RefreshLocal(int X, int Y, int right, int bottom)
                {
                    for (var y = Y;  y < bottom; y++)
                    for (var x = X; x < right; x++)
                    {
                        Refresh(x, y);
                    }
                }

                public void Refresh(int x, int y)
                {
                    if(x < 0 || x >= _Width || y < 0 || y >= _Height) return;

                    var vx = x % _Width;      
                    var vy = y % _Height;
                    if (vx < 0) vx += _Width;
                    if (vy < 0) vy += _Height;

                    var index = (vx+vy * _Width) * 4 * Layers;
                    var rec = new FloatRect(x * TileSize, y * TileSize, TileSize, TileSize);

                    for (int i = 0; i < Layers; i++)
                    {
                        Color color;
                        IntRect src;
                        _Provider(x, y, i, out color, out src);

                        index = UpdateVerticies(index, rec, src, color);
                    }
                }

        private unsafe int UpdateVerticies(int index, FloatRect rec, IntRect src, Color color)
        {
            fixed (Vertex* fptr = _Vertices)
            {
                Vertex* ptr = fptr + index;

                ptr.Position.X = rec.X;
                ptr.Position.Y = rec.Y;
                ptr.TexCoords.X = src.X;
                ptr.TexCoords.Y = src.Y;
                ptr.Color = color;
                ptr++;

                ptr.Position.X = rec.X + rec.Width;
                ptr.Position.Y = rec.Y;
                ptr.TexCoords.X = src.X + src.Width;
                ptr.TexCoords.Y = src.Y;
                ptr.Color = color;
                ptr++;

                ptr.Position.X = rec.X + rec.Width;
                ptr.Position.Y = rec.Y + rec.Height;
                ptr.TexCoords.X = src.X + src.Width;
                ptr.TexCoords.Y = src.Y + src.Height;
                ptr.Color = color;
                ptr++;

                ptr.Position.X = rec.X;
                ptr.Position.Y = rec.Y + rec.Height;
                ptr.TexCoords.X = src.X;
                ptr.TexCoords.Y = src.Y + src.Height;
                ptr.Color = color;
                return (int)ptr++;
            }
        }
        */

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