using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;
using BlackCoat.Entities;
using BlackCoat.Entities.Lights;
using BlackCoat.Entities.Shapes;
using BlackTournament.Entities;
using BlackTournament.Net.Data;
using BlackTournament.Tmx;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace BlackTournament.Scenes
{
    class LightEditor: Scene
    {
        private View _View;
        private TmxMapper _MapData;
        private Lightmap _Lightmap;
        private IPropertyInspector _Inspector;
        private Rectangle _Selection;

        public LightEditor(Game game, string map) : base(game.Core, $"Light Editor: {map}", Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
            _MapData = new TmxMapper(() => 0);
            if (!_MapData.Load(map, _Core.CollisionSystem)) throw new ArgumentException(map);
        }


        private void UpdateViewOnDeviceResize(Vector2f size) => _View.Size = size;

        protected override bool Load()
        {
            // Setup View
            _View = new View(new FloatRect(new Vector2f(), _Core.DeviceSize));
            Layer_BG.View = _View;
            Layer_Game.View = _View;
            Layer_Overlay.View = _View;

            _Core.DeviceResized += UpdateViewOnDeviceResize;
            _Core.Input.MouseButtonPressed += HandleMouseButtonPressed;
            _Core.Input.KeyPressed += HandleKeyPressed;


            // Setup Map
            _Core.ClearColor = _MapData.ClearColor;
            foreach (var layer in _MapData.Layers)
            {
                switch (layer)
                {
                    case TileLayer tileLayer: // MAP TILES
                        var mapTex = TextureLoader.Load(tileLayer.Asset);
                        var mapLayer = new MapRenderer(_Core, _MapData.Size, mapTex, _MapData.TileSize)
                        {
                            Position = tileLayer.Offset
                        };

                        for (int i = 0; i < tileLayer.Tiles.Length; i++)
                        {
                            mapLayer.AddTile(i * 4, tileLayer.Tiles[i].Position, tileLayer.Tiles[i].TexCoords);
                        }
                        Layer_BG.Add(mapLayer);
                        break;

                    case ObjectLayer objectLayer: // skipped
                        break;
                }
            }
            //TODO: add marker 4 pickup locations

            // Setup Lighting
            _Lightmap = new Lightmap(_Core, new Vector2f(_MapData.TileSize.X * _MapData.Size.X, _MapData.TileSize.Y * _MapData.Size.Y));
            _Lightmap.RenderEachFrame = true;
            var lightFile = $"Maps\\{_MapData.Name}.bcl";
            if (File.Exists(lightFile))
            {
                _Lightmap.Load(TextureLoader, lightFile);
            }
            else
            {
                Log.Warning("No Lighting File for", _MapData.Name);
            }
            Layer_Overlay.Add(_Lightmap);


            _Inspector = OpenInspector();
            _Inspector.InspectionItemChanged += Inspector_InspectionItemChanged;

            Layer_Overlay.Add(_Selection = new Rectangle(_Core, Color.Transparent)
            {
                OutlineColor = Color.Magenta,
                OutlineThickness = 2
            });
            return true;
        }

        private void Inspector_InspectionItemChanged(object obj)
        {
            _Selection.Visible = false;

            if (obj is IEntity e)
            {
                _Selection.Visible = true;
                _Selection.Position = e.Position;
                _Selection.Origin = e.Origin.MultiplyBy(e.Scale);
                _Selection.Size = 25.ToVector2f(); // backup default value to show there is "something"

                switch (e)
                {
                    case Graphic g:
                        _Selection.Size = g.TextureRect.Size().ToVector2f().MultiplyBy(g.Scale);
                    break;
                    case Rectangle r:
                        _Selection.Size = r.Size;
                    break;
                    case Circle c:
                        _Selection.Size = (c.Radius * 2).ToVector2f();
                        break;
                }
            }

            // Move view to selected item
            var viewRect = new RectangleCollisionShape(_Core.CollisionSystem, _View.Center - _View.Size / 2, _View.Size);
            var selectionRect = new RectangleCollisionShape(_Core.CollisionSystem, _Selection.Position - _Selection.Origin, _Selection.Size);
            if (!viewRect.CollidesWith(selectionRect))
            {
                _View.Center = selectionRect.Position + selectionRect.Size / 2;
            }
        }

        private void HandleMouseButtonPressed(Mouse.Button btn)
        {
            if (btn == Mouse.Button.Left)
            {
                var globalMousePos = _Core.Input.MousePosition + _View.Center - _View.Size / 2;
                var target = _Lightmap.GetAll<Container>()
                            .SelectMany(c => c.GetAll<Graphic>())
                            .Where(g => g.CollisionShape.CollidesWith(globalMousePos))
                            .OrderBy(g => g.Position.DistanceBetweenSquared(globalMousePos))
                            .FirstOrDefault();
                _Inspector.InspectionItem = target;
            }
        }

        private void HandleKeyPressed(Keyboard.Key key)
        {
            if (key == Keyboard.Key.Delete)
            {
                _Inspector.RemoveCurrent();
                _Selection.Visible = false;
            }
        }

        protected override void Update(float deltaT)
        {
            var move = new Vector2f();
            if (Input.DEFAULT.IsKeyDown(Keyboard.Key.W)) move.Y = -1;
            else if (Input.DEFAULT.IsKeyDown(Keyboard.Key.S)) move.Y = 1;
            if (Input.DEFAULT.IsKeyDown(Keyboard.Key.A)) move.X = -1;
            else if (Input.DEFAULT.IsKeyDown(Keyboard.Key.D)) move.X = 1;
            _View.Center += move * 2000 * deltaT;

            if (Input.DEFAULT.IsMButtonDown(Mouse.Button.Middle) && _Inspector.InspectionItem is IEntity entity)
            {
                _Selection.Position = entity.Position = Input.DEFAULT.MousePosition + _View.Center - _View.Size / 2;
            }
        }

        protected override void Destroy()
        {
            _Core.DeviceResized -= UpdateViewOnDeviceResize;
            _Core.Input.MouseButtonPressed -= HandleMouseButtonPressed;
            _Core.Input.KeyPressed -= HandleKeyPressed;
        }
    }
}