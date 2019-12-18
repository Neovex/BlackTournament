using System;
using SFML.System;

namespace BlackTournament.Tmx
{
    abstract class Layer
    {
        public Vector2f Offset { get; internal set; }
        public int RenderOrder { get; internal set; } = int.MaxValue;
    }

    class TileLayer : Layer
    {
        public string Asset { get; internal set; }
        public Tile[] Tiles { get; internal set; }
    }

    class ObjectLayer : Layer
    {
        public ActorInfo[] Objects { get; internal set; }
    }
}