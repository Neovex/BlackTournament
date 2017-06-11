using SFML.System;

namespace BlackTournament.Tmx
{
    class Tile
    {
        public Vector2f Position { get; private set; }
        public Vector2i TexCoords { get; private set; }

        public Tile(Vector2f pos, Vector2i tex)
        {
            Position = pos;
            TexCoords = tex;
        }
    }
}