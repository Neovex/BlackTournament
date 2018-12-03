using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.System;
using SFML.Audio;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.UI;
using BlackCoat.Entities;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;
using BlackCoat.Entities.Shapes;

using BlackTournament.Entities;


namespace BlackTournament.GameStates
{
    class MainMenu:Gamestate
    {
        private Music music;

        private Graphic _Background;
        private Graphic _MenueGore;

        private PrerenderedContainer _Lighting;
        private Graphic _EffectLight;
        private Rectangle _AmbientLight;

        private UICanvas _UI;
        private Button _NewGameButton;
        private Button _ExitButton;
        private Rectangle _Debug;

        public MainMenu(Core core):base(core, nameof(MainMenu), Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            // Music
            music = MusicLoader.Load("Ten_Seconds_to_Rush");
            music.Volume = 15;
            //music.Play();

            // BG
            Layer_BG.Add(_Background = new Graphic(_Core, TextureLoader.Load(Files.Menue_Bg, true)));
            Layer_BG.Add(_MenueGore = new Graphic(_Core, TextureLoader.Load(Files.Menue_Gore)) { BlendMode = BlendMode.Multiply });
            // BG Lighting
            Layer_BG.Add(_Lighting = new PrerenderedContainer(_Core) { BlendMode = BlendMode.Multiply, ClearColor = Color.Black, RedrawEachFrame = true });
            _Lighting.Add(_EffectLight = new Graphic(_Core, TextureLoader.Load(Files.Emitter_Smoke_White)) { Visible = false });
            _Lighting.Add(_AmbientLight = new Rectangle(_Core, Color.White) { BlendMode = BlendMode.Add, Alpha = 0.05f });

            // UI
            var tex = TextureLoader.Load(Files.Menue_Panel);
            var buttonTex = TextureLoader.Load(Files.Menue_Button, false, true);
            Layer_Game.Add
            (
                _UI = new UICanvas(_Core, tex.Size.ToVector2f())
                {
                    Texture = tex,
                    Input = new UIInput(_Core.Input, true),
                    Init = new UIComponent[]
                    {
                        new AnchoredContainer(_Core, Anchor.Center)
                        {
                            Init = new UIComponent[]
                            {
                                new OffsetContainer(_Core, false)
                                {
                                    Offset = 25,
                                    Init = new UIComponent[]
                                    {
                                        _NewGameButton = new Button(_Core, buttonTex.Size.ToVector2f())
                                        {
                                            Texture = buttonTex,
                                            InitPressed = ButtonPressed,
                                            InitReleased = ButtonClicked
                                        },
                                        _ExitButton = new Button(_Core, buttonTex.Size.ToVector2f())
                                        {
                                            Texture = buttonTex,
                                            InitPressed = ButtonPressed,
                                            InitReleased = ButtonClicked
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );

            // System
            _Core.DeviceResized += HandleCoreDeviceResized;
            HandleCoreDeviceResized(_Core.DeviceSize);

            // DELME
            OpenInspector();
            Layer_Debug.Add(_Debug = new Rectangle(_Core, Color.Magenta) { Alpha = 0.5f });

            return true;
        }

        private void ButtonPressed(Button button)
        {
            button.Texture = TextureLoader.Load(Files.Menue_Button_Active, false, true);
            _Debug.Position = (button.CollisionShape as UICollisionShape).Position;
            _Debug.Size = (button.CollisionShape as UICollisionShape).Size;
        }

        private void ButtonClicked(Button button)
        {
            button.Texture = TextureLoader.Load(Files.Menue_Button, false, true);
        }

        private void HandleCoreDeviceResized(Vector2f size)
        {
            _Background.TextureRect = new IntRect(default(Vector2i), size.ToVector2i());
            _MenueGore.Position = new Vector2f(size.X/2 - _MenueGore.Texture.Size.X / 2, 0);
            _AmbientLight.Size = size;
            _Lighting.RedrawNow();
            _UI.Position = size / 2 - _UI.OuterSize / 2;
        }

        internal void DisplayPopupMessage(string message)
        {
            Log.Debug(message);
        }

        protected override void Update(float deltaT)
        {
        }

        protected override void Destroy()
        {
            music?.Dispose();
            // todo: find better destroy / music logic - I think this is already done by the music asset manager but better double check
            //_Core.AnimationManager.Run(music.Volume, 0, 1, v => music.Volume = v, BlackCoat.Animation.InterpolationType.Linear, a => music = null);
        }
    }
}