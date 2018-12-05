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
using BlackTournament.UI;

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
        private Int32 _Shots;
        private Sound _SfxShot;

        private UICanvas _UI;
        private BigButton _BrowseButton;
        private BigButton _HostButton;
        private BigButton _CreditsButton;
        private BigButton _ExitButton;
        private Rectangle _Debug;

        // Animation


        public MainMenu(Core core):base(core, nameof(MainMenu), Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            // Music
            music = MusicLoader.Load(Files.MENUE_MUSIC.Skip(_Core.Random.Next(Files.MENUE_MUSIC.Count)).First());
            music.Volume = 25;
            music.Play();

            // BG
            Layer_BG.Add(_Background = new Graphic(_Core, TextureLoader.Load(Files.Menue_Bg, true)));
            Layer_BG.Add(_MenueGore = new Graphic(_Core, TextureLoader.Load(Files.Menue_Gore)) { BlendMode = BlendMode.Multiply });
            // BG Lighting
            Layer_BG.Add(_Lighting = new PrerenderedContainer(_Core) { BlendMode = BlendMode.Multiply, ClearColor = Color.Black, RedrawEachFrame = true });
            _Lighting.Add(_EffectLight = new Graphic(_Core, TextureLoader.Load(Files.Emitter_Smoke_White)) { Visible = false, Origin = Create.Vector2f(100) });
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
                                        _BrowseButton = new BigButton(_Core, TextureLoader,"Browse Servers")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _HostButton = new BigButton(_Core, TextureLoader,"Host Game")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _CreditsButton = new BigButton(_Core, TextureLoader,"Credits")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _ExitButton = new BigButton(_Core, TextureLoader,"Exit")
                                        {
                                            InitReleased = ButtonClicked
                                        }// Add missing UI decors
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

            // Animation
            _Core.AnimationManager.Wait(4, StartAnimation);
            _SfxShot = new Sound(SfxLoader.Load(Files.Sfx_Simpleshot));
            _SfxShot.Volume = 25;

            // DELME
            OpenInspector();

            return true;
        }

        private void ButtonClicked(Button button)
        {
            if (button == _ExitButton) _Core.Exit("Exit by menu");
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

        private void StartAnimation()
        {
            if (Destroyed) return;
            _Shots = _Core.Random.Next(4, 8);
            _EffectLight.Rotation = _Core.Random.NextFloat(0, 360);
            _EffectLight.Scale = Create.Vector2f(_Core.Random.NextFloat(8.5f, 14.4f));
            _EffectLight.Position = new Vector2f(_Core.DeviceSize.X * _Core.Random.Next(2), _Core.DeviceSize.Y * _Core.Random.Next(2));
            Shot();
        }

        private void Shot()
        {
            if (Destroyed) return;
            if (_EffectLight.Visible)
            {
                _EffectLight.Visible = false;
                _Core.AnimationManager.Wait(WeaponData.DrakePrimary.FireRate, Shot);
            }
            else if (_Shots != 0)
            {
                _Shots--;
                _SfxShot.Play();
                _EffectLight.Visible = true;
                _Core.AnimationManager.Wait(WeaponData.DrakePrimary.FireRate, Shot);
            }
            else
            {
                _Core.AnimationManager.Wait(_Core.Random.Next(3, 8), StartAnimation);
            }
        }

        protected override void Destroy()
        {
            music?.Dispose();
            // todo: find better destroy / music logic - I think this is already done by the music asset manager but better double check
            //_Core.AnimationManager.Run(music.Volume, 0, 1, v => music.Volume = v, BlackCoat.Animation.InterpolationType.Linear, a => music = null);
        }
    }
}