using System;
using SFML.System;

namespace BlackTournament.Tmx
{
    class Layer
    {
        public String TextureName { get; internal set; }
        public Vector2f Offset { get; internal set; }
        public Tile[] Tiles { get; internal set; }
    }
}