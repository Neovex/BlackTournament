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


        public MapState(Core core, TmxMapper map) : base(core, map.Name)
        {
            _MapData = map ?? throw new ArgumentNullException(nameof(map));
            _EnitityLookup = new Dictionary<int, IEntity>();
        }

        protected override bool Load()
        {
            TextureManager.RootFolder = "Assets";

            // Setup View
            _View = new View(new FloatRect(new Vector2f(), _Core.DeviceSize.ToVector2f()));
            Layer_BG.View = _View;

            // Setup Map
            _Core.ClearColor = _MapData.ClearColor;
            foreach (var layer in _MapData.TileLayers)
            {
                var mapLayer = new MapRenderer(_Core, _MapData.Size, TextureManager.Load(layer.TextureName), _MapData.TileSize);
                mapLayer.Position = layer.Offset;

                //_View.Center = 

                int i = 0;
                foreach (var tile in layer.Tiles)
                {
                    mapLayer.AddTile(i * 4, tile.Position, tile.TexCoords); // mayhaps find a better solution
                    i++;
                }
                Layer_BG.AddChild(mapLayer);
            }

            // TESTING ############################################

            // Debug Views
            var wallColor = new Color(155, 155, 155, 155); // TODO : maybe add remaining debug views
            foreach (var shape in _MapData.WallCollider)
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
            foreach (var pos in _MapData.SpawnPoints)
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
                Scale = new Vector2f(0.5f, 0.5f) // FIXME?
            };
            player.Origin = player.Texture.Size.ToVector2f() / 2;
            Layer_Game.AddChild(player);
            _EnitityLookup.Add(id, player);

            if (isLocalPlayer) _LocalPlayer = player;
        }

        public void CreateEntity(int id, PickupType type, Vector2f pos, float rotation)
        {
            // TODO
        }

        public void CreateVFX(object type, Vector2f pos, float rotation)
        {
            // TODO
        }

        public void Destroy(int id)
        {
            var entity = _EnitityLookup[id];
            entity.Parent.RemoveChild(entity);
            _EnitityLookup.Remove(id);
        }

        public void UpdateEntity(int id, Vector2f pos, float rotation, bool visible) // add pickup type?
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