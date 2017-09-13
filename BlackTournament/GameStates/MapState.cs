using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.System;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Collision.Shapes;

using BlackTournament.Entities;
using BlackTournament.Tmx;
using BlackCoat.Entities.Shapes;
using BlackTournament.Net.Data;

namespace BlackTournament.GameStates
{
    class MapState : BaseGamestate
    {
        private TmxMapper _MapData;

        private View _View;
        private Dictionary<int, IEntity> _EnitityLookup;
        private IEntity _LocalPlayer;


        public Vector2f ViewMovement { get; set; }


        public MapState(Core core, TmxMapper map) : base(core, map.Name, Game.ASSET_ROOT)
        {
            _MapData = map ?? throw new ArgumentNullException(nameof(map));
            _EnitityLookup = new Dictionary<int, IEntity>();
        }

        protected override bool Load()
        {
            // Setup View
            _View = new View(new FloatRect(new Vector2f(), _Core.DeviceSize.ToVector2f()));
            Layer_BG.View = _View;

            // Setup Map
            _Core.ClearColor = _MapData.ClearColor;
            foreach (var layer in _MapData.TileLayers)
            {
                var mapTex = TextureManager.Load(layer.TextureName);
                var mapLayer = new MapRenderer(_Core, _MapData.Size, mapTex, _MapData.TileSize);
                mapLayer.Position = layer.Offset;

                int i = 0;
                foreach (var tile in layer.Tiles)
                {
                    mapLayer.AddTile(i * 4, tile.Position, tile.TexCoords); // mayhaps find a better solution
                    i++;
                }
                Layer_BG.AddChild(mapLayer);
            }

            // TODO: add center pos to mapdata
            _View.Center = _MapData.Pickups.FirstOrDefault(p => p.Type == PickupType.BigShield)?.Position ?? _View.Center;

            // TESTING ############################################

            // Debug Views
            var wallColor = new Color(155, 155, 155, 155);
            foreach (var shape in _MapData.WallCollider) // Collision
            {
                if (shape is RectangleCollisionShape)
                {
                    var s = (RectangleCollisionShape)shape;
                    Layer_BG.AddChild(new Rectangle(_Core)
                    {
                        Position = s.Position,
                        Size = s.Size,
                        FillColor = wallColor,
                        OutlineColor = Color.Black,
                        OutlineThickness = 1
                    });
                }
                else if (shape is PolygonCollisionShape)
                {
                    var s = (PolygonCollisionShape)shape;
                    Layer_BG.AddChild(new Polygon(_Core, s.Points)
                    {
                        Position = s.Position,
                        FillColor = wallColor,
                        OutlineColor = Color.Black,
                        OutlineThickness = 1
                    });
                }
            }
            foreach (var pos in _MapData.SpawnPoints) // Spawns
            {
                Layer_BG.AddChild(new Rectangle(_Core)
                {
                    Position = pos,
                    Size = new Vector2f(10, 10),
                    Origin = new Vector2f(5, 5),
                    OutlineThickness = 1,
                    OutlineColor = Color.Blue,
                    FillColor = Color.White,
                    Alpha = 0.3f
                });
            }
            return true;
        }

        protected override void Update(float deltaT)
        {
            _View.Center += ViewMovement * 2000 * deltaT; // spectator view movement
        }

        protected override void Destroy()
        {
        }

        public void CreatePlayer(int id, bool isLocalPlayer = false)
        {
            var player = new Graphic(_Core)
            {
                Texture = TextureManager.Load("CharacterBase"),
                Color = Color.Red,
                View = _View,
                Scale = new Vector2f(0.5f, 0.5f) // FIXME!
            };
            player.Origin = player.Texture.Size.ToVector2f() / 2;
            Layer_Game.AddChild(player);
            _EnitityLookup.Add(id, player);

            if (isLocalPlayer) _LocalPlayer = player;
        }

        public void CreatePickup(int id, PickupType type, Vector2f position, bool visible)
        {
            var tex = TextureManager.Load(type.ToString());
            var entity = new Graphic(_Core)
            {
                Texture = tex,
                Scale = new Vector2f(0.4f, 0.4f), // FIXME!
                View = _View,
                Position = position,
                Origin = tex.Size.ToVector2f() / 2,
                Visible = visible
            };
            _EnitityLookup.Add(id, entity);
            Layer_Game.AddChild(entity);
        }

        public void CreateShot(int id, PickupType type, Vector2f position, float rotation)
        {
            var entity = new Rectangle(_Core) // todo : replace debug view with proper shells/efx
            {
                Position = position,
                Size = new Vector2f(250, 2),
                Color = Color.Red
            };
            _EnitityLookup.Add(id, entity);
            Layer_Game.AddChild(entity);
        }

        public void CreateVFX(PickupType type, Vector2f pos, float rotation)
        {
            // TODO
        }

        public void DestroyEntity(int id)
        {
            var entity = _EnitityLookup[id];
            entity.Parent.RemoveChild(entity);
            _EnitityLookup.Remove(id);
        }

        public void UpdateEntity(int id, Vector2f pos, float rotation, bool visible) // add pickup type? currently not necessary
        {
            var entity = _EnitityLookup[id];
            entity.Position = pos;
            entity.Rotation = rotation;
            entity.Visible = visible;
        }

        public void FocusPlayer()
        {
            _View.Center = _LocalPlayer.Position;
        }

        public void RotatePlayer(float angle)
        {
            _LocalPlayer.Rotation = angle;
            //_View.Rotation = -_Player.Rotation; // cool effect - see later if this can be used (with game-pads maybe)
        }

        public override string ToString()
        {
            return $"Map: \"{Name}\"";
        }
    }
}