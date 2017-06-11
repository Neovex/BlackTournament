using BlackTournament.Net.Data;
using SFML.System;

namespace BlackTournament.Tmx
{
    class Pickup
    {
        public Vector2f Position { get; private set; }
        public PickupType Type { get; private set; }
        public int Amount { get; private set; }
        public float RespawnTime { get; private set; }

        public Pickup(Vector2f position, PickupType type, int amount, float respawnTime)
        {
            Position = position;
            Type = type;
            Amount = amount;
            RespawnTime = respawnTime;
        }
    }
}