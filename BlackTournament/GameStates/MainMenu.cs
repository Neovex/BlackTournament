using System;
using System.Collections.Generic;
using System.Linq;

using SFML.System;
using SFML.Audio;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.UI;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;

using BlackTournament.UI;
using BlackTournament.Properties;
using System.Net;

namespace BlackTournament.GameStates
{
    class MainMenu:Gamestate
    {
        public event Action Browse = () => { };
        public event Action Host = () => { };

        // System
        private EndPoint _SelectedServer;

        // Audio
        private Music music;
        private SfxManager _Sfx;
        // BG
        private Graphic _Background;
        private Graphic _MenueGore;
        // BG Light
        private PrerenderedContainer _Lighting;
        private Graphic _EffectLight;
        private Rectangle _AmbientLight;
        // Animation
        private Int32 _Shots;
        // UI
        private Canvas _MainUI;
        private AlignedContainer _MainUIContent;
        private BigButton _BrowseButton;
        private BigButton _HostButton;
        private BigButton _CreditsButton;
        private BigButton _ExitButton;

        private AlignedContainer _HostDialog;
        private TextBox _HostMapNameTempTextBox;
        private TextBox _HostServerNameTextBox;
        private TextBox _HostPortTextBox;
        private BigButton _HostCancelButton;
        private BigButton _HostHostButton;

        private Canvas _ServerBrowser;
        private OffsetContainer _ServerList;
        private BigButton _BrowseRefreshButton;
        private BigButton _BrowseBackButton;
        private BigButton _BrowseDirectConnectButton;

        private BigButton _BrowseJoinButton;

        private Canvas _Credits;
        private BigButton _CreditsBackButton;
        
        // Overlay decors
        private Graphic _Glow;
        private Graphic _Title;
        private Graphic _Logo;


        // Properties
        public string Mapname => _HostMapNameTempTextBox?.Text;
        public string Servername => _HostServerNameTextBox?.Text;
        public int Port => Int32.TryParse(_HostPortTextBox?.Text, out int r) ? r : -1;


        // CTOR
        public MainMenu(Core core):base(core, nameof(MainMenu), Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }


        protected override bool Load()
        {
            // Music
            music = MusicLoader.Load(Files.MENUE_MUSIC.Skip(_Core.Random.Next(Files.MENUE_MUSIC.Count)).First());
            music.Volume = Settings.Default.MusikVolume;
            music.Loop = true; // TODO : consider music manager 4 looping play-lists etc
            //music.Play();

            // Sfx
            _Sfx = new SfxManager(SfxLoader);
            _Sfx.AddToLibrary(Files.Sfx_Simpleshot, Settings.Default.SfxVolume / 2);

            // BG
            Layer_BG.Add
            (
                _Background = new Graphic(_Core, TextureLoader.Load(Files.Menue_Bg, true)),
                _MenueGore = new Graphic(_Core, TextureLoader.Load(Files.Menue_Gore)) { BlendMode = BlendMode.Multiply },
                // BG Lighting
                _Lighting = new PrerenderedContainer(_Core) { BlendMode = BlendMode.Multiply, ClearColor = Color.Black, RedrawEachFrame = true }
            );
            _Lighting.Add(_EffectLight = new Graphic(_Core, TextureLoader.Load(Files.Emitter_Smoke_White)) { Visible = false, Origin = Create.Vector2f(100) });
            _Lighting.Add(_AmbientLight = new Rectangle(_Core, Color.White) { BlendMode = BlendMode.Add, Alpha = 0.05f });

            // UI
            var tex = TextureLoader.Load(Files.Menue_Panel);
            var buttonTex = TextureLoader.Load(Files.Menue_Button, false, true);
            Layer_Game.Add
            (
                // MAIN
                _MainUI = new Canvas(_Core, tex.Size.ToVector2f())
                {
                    Texture = tex,
                    Input = new UIInput(_Core.Input, true),
                    Init = new UIComponent[]
                    {
                        _MainUIContent = new AlignedContainer(_Core, Alignment.Center)
                        {
                            Init = new UIComponent[]
                            {
                                new OffsetContainer(_Core, false)
                                {
                                    Offset = 25,
                                    Init = new UIComponent[]
                                    {
                                        _BrowseButton = new BigButton(_Core, TextureLoader, _Sfx, "Browse Servers")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _HostButton = new BigButton(_Core, TextureLoader, _Sfx,"Host Game")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _CreditsButton = new BigButton(_Core, TextureLoader, _Sfx,"Credits")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _ExitButton = new BigButton(_Core, TextureLoader, _Sfx,"Exit")
                                        {
                                            InitReleased = ButtonClicked
                                        }
                                    }
                                }
                            }
                        },
                        _HostDialog = new AlignedContainer(_Core, Alignment.Center)
                        {
                            Visible = false,
                            Init = new UIComponent[]
                            {
                                new OffsetContainer(_Core, false)
                                {
                                    Offset = 8,
                                    Init = new UIComponent[]
                                    {
                                        new Label(_Core, "Map", 16, Game.DefaultFont),
                                        _HostMapNameTempTextBox = new TextBox(_Core, new Vector2f(205, 19), 16, Game.DefaultFont)
                                        {
                                            Text = "TODO Combobox",             // TODO : Combobox
                                            Margin = new FloatRect(0,0,0,15),
                                            Padding = new FloatRect(5,5,0,0),
                                        },
                                        new Label(_Core, "Server Name", 16, Game.DefaultFont),
                                        _HostServerNameTextBox = new TextBox(_Core, new Vector2f(205, 19), 16, Game.DefaultFont)
                                        {
                                            Text = $"{Settings.Default.PlayerName}'s Server",
                                            Margin = new FloatRect(0,0,0,15),
                                            Padding = new FloatRect(5,5,0,0),
                                        },
                                        new Label(_Core, "Port", 16, Game.DefaultFont),
                                        _HostPortTextBox = new TextBox(_Core, new Vector2f(205, 19), 16, Game.DefaultFont)
                                        {
                                            Text = Net.Net.DEFAULT_PORT.ToString(),
                                            Margin = new FloatRect(0,0,0,15),
                                            Padding = new FloatRect(5,5,0,0),
                                            InitFocusLost = PortLimit,
                                            InitTextChanged = PortFilter
                                        },
                                        _HostHostButton = new BigButton(_Core, TextureLoader, _Sfx,"Host")
                                        {
                                            InitReleased = ButtonClicked
                                        },
                                        _HostCancelButton = new BigButton(_Core, TextureLoader, _Sfx,"Cancel")
                                        {
                                            Margin = new FloatRect(0,10,0,0),
                                            InitReleased = ButtonClicked
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                // SERVER BROWSER
                _ServerBrowser = new Canvas(_Core, new Vector2f(700, 540))
                {
                    Visible = false,
                    Input = _MainUI.Input,
                    BackgroundAlpha = 1,
                    BackgroundColor = new Color(180, 0, 0),
                    Init = new UIComponent[]
                    {
                        new Canvas(_Core)
                        {
                            DockX = true, DockY = true,
                            Margin = new FloatRect(1,1,1,1),
                            BackgroundColor = Color.Black,
                            BackgroundAlpha = 0.85f,
                            Texture=TextureLoader.Load(Files.Menue_Bg2, true),
                            Init = new UIComponent[]
                            {
                                new DistributionContainer(_Core, false)
                                {
                                    DockX = true,
                                    Margin = new FloatRect(10,10,10,10),
                                    Init = new UIComponent[]
                                    {
                                        new DistributionContainer(_Core,true)
                                        {
                                            Init = new UIComponent[]
                                            {
                                                _BrowseBackButton = new BigButton(_Core, TextureLoader, _Sfx, "Back")
                                                {
                                                    InitReleased = ButtonClicked
                                                },
                                                _BrowseRefreshButton = new BigButton(_Core, TextureLoader, _Sfx, "Refresh")
                                                {
                                                    InitReleased = ButtonClicked
                                                },
                                            }
                                        },
                                        new Canvas(_Core, new Vector2f(100, 100))
                                        {
                                            DockX = true, DockY = true,
                                            Margin = new FloatRect(5,10,5,10),
                                            BackgroundColor = Color.White,
                                            BackgroundAlpha = 0.03f,
                                            Init = new UIComponent[]
                                            {
                                                _ServerList = new OffsetContainer(_Core, false)
                                                {
                                                    DockX = true
                                                }
                                            }
                                        },
                                        new DistributionContainer(_Core,true)
                                        {
                                            Init = new UIComponent[]
                                            {
                                                _BrowseDirectConnectButton = new BigButton(_Core, TextureLoader, _Sfx, "Direct Connect")
                                                {
                                                    InitReleased = ButtonClicked
                                                },
                                                _BrowseJoinButton = new BigButton(_Core, TextureLoader, _Sfx, "Join")
                                                {
                                                    Enabled = false,
                                                    InitReleased = ButtonClicked
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    }
                },
                // CREDITS
                _Credits = new Canvas(_Core, new Vector2f(700, 540))
                {
                    Visible = false,
                    Input = _MainUI.Input,
                    BackgroundAlpha = 1,
                    BackgroundColor = new Color(180, 0, 0),
                    Init = new UIComponent[]
                    {
                        new Canvas(_Core)
                        {
                            DockX = true, DockY = true,
                            Margin = new FloatRect(1,1,1,1),
                            BackgroundColor = Color.Black,
                            BackgroundAlpha = 0.85f,
                            Texture=TextureLoader.Load(Files.Menue_Bg2, true),
                            Init = new UIComponent[]
                            {
                                new ScrollContainer(_Core)
                                {
                                    DockX = true, DockY = true,
                                    Init = new UIComponent[]
                                    {
                                        new Label(_Core, System.IO.File.ReadAllText("Assets\\Credits.txt").Replace("\r",""), font:Game.DefaultFont)
                                        {
                                            CharacterSize = 12,
                                            Position = Create.Vector2f(10),
                                            TextColor = new Color(0,100,255)
                                        }
                                    }
                                },
                                _CreditsBackButton = new BigButton(_Core, TextureLoader, _Sfx, "Back")
                                {
                                    Position = new Vector2f(700-buttonTex.Size.X-10,10),
                                    InitReleased = ButtonClicked
                                }
                            }
                        }
                    }
                }
            );
            // Decors
            Layer_Overlay.Add
            (
                _Glow = new Graphic(_Core, TextureLoader.Load(Files.Menue_Glow))
                {
                    BlendMode = BlendMode.Add
                },
                _Title = new Graphic(_Core, TextureLoader.Load(Files.Menue_Title)),
                _Logo = new Graphic(_Core, TextureLoader.Load(Files.Menue_Logo))
                {
                    Origin = new Vector2f(345, 355),
                    Scale = Create.Vector2f(0.25f)
                }
            );

            // System
            _Core.DeviceResized += HandleCoreDeviceResized;
            HandleCoreDeviceResized(_Core.DeviceSize);
            // Background Animation
            _Core.AnimationManager.Wait(4, StartAnimation);

            return true;
        }

        private void ButtonClicked(Button button)
        {
            //Main
            if (button == _BrowseButton) OpenServerBrowser(true);
            if (button == _HostButton) OpenHostUI(true);
            if (button == _HostCancelButton) OpenHostUI(false);
            if (button == _HostHostButton) Host.Invoke();
            if (button == _CreditsButton) OpenCredits(true);
            if (button == _CreditsBackButton) OpenCredits(false);
            if (button == _ExitButton) _Core.Exit("Exit by menu");
            // Server Browser
            if (button == _BrowseBackButton) OpenServerBrowser(false);
            if (button == _BrowseRefreshButton) Browse.Invoke();
            if (button == _BrowseDirectConnectButton) { } // TODO : implement
            if (button == _BrowseJoinButton) { } // TODO : connect
        }

        private void OpenHostUI(bool open)
        {
            _MainUIContent.Visible = !open;
            _HostDialog.Visible = open;
        }

        private void OpenCredits(bool open)
        {
            _MainUI.Visible = _Logo.Visible = !open;
            _Credits.Visible = open;
        }

        internal void OpenServerBrowser(bool open)
        {
            _MainUI.Visible = _Logo.Visible = !open;
            _ServerBrowser.Visible = open;
        }
        private void PortLimit(UIComponent component)
        {
            var label = component as Label;
            if (Port < 1025 || Port > 65535) label.Text = Net.Net.DEFAULT_PORT.ToString();
        }

        private void PortFilter(Label label)
        {
            if (!label.Text.All(c => Char.IsDigit(c))) label.Text = new string(label.Text.Where(c => Char.IsDigit(c)).ToArray());
        }

        private void HandleCoreDeviceResized(Vector2f size)
        {
            _Background.TextureRect = new IntRect(default(Vector2i), size.ToVector2i());
            _MenueGore.Position = new Vector2f(size.X/2 - _MenueGore.Texture.Size.X / 2, 0);
            _AmbientLight.Size = size;
            _Lighting.RedrawNow();
            _MainUI.Position = new Vector2f(size.X/2,size.Y *0.6f) - _MainUI.OuterSize / 2;
            _ServerBrowser.Position = new Vector2f(size.X/2,size.Y *0.6f) - _ServerBrowser.OuterSize / 2;
            _Credits.Position = _ServerBrowser.Position;

            _Title.Scale = Create.Vector2f(Math.Min(Math.Min(1, size.X / _Title.TextureRect.Width),
                                                    Math.Min(1, _MainUI.Position.Y / _Title.TextureRect.Height)));
            _Title.Position = new Vector2f(size.X / 2 - _Title.TextureRect.Width * _Title.Scale.X / 2, _MainUI.Position.Y / 2 - _Title.TextureRect.Height * _Title.Scale.Y / 2);
            _Glow.Scale = new Vector2f(1, _MainUI.Position.Y / _Glow.TextureRect.Height * -1.5f);
            _Glow.Position = new Vector2f(size.X / 2 - _Glow.TextureRect.Width / 2, _Glow.TextureRect.Height * -_Glow.Scale.Y);
            _Logo.Position = size - Create.Vector2f(150);
        }

        internal void DisplayPopupMessage(string message)
        {
            Log.Debug(message); // TODO : implement
        }

        internal void UpdateServerList((IPEndPoint, Net.Data.ServerInfo)[] server)
        {
            foreach (ServerInfo info in _ServerList.Components)
            {
                info.Checked -= HandleServerSelected;
            }
            _ServerList.Clear();
            foreach (var s in server)
            {
                var info = new ServerInfo(_Core, _Sfx, s);
                info.Checked += HandleServerSelected;
                _ServerList.Add(info);
            }
        }

        private void HandleServerSelected(ServerInfo server)
        {
            _SelectedServer = server?.Endpoint;
            _BrowseJoinButton.Enabled = server != null;
        }

        protected override void Update(float deltaT)
        {
            _Logo.Rotation += 30 * deltaT;
        }

        private void StartAnimation()
        {
            if (Destroyed) return;
            _Shots = _Core.Random.Next(6, 12);
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
                _Core.AnimationManager.Wait(WeaponData.DrakePrimary.FireRate / 2, Shot);
            }
            else if (_Shots != 0)
            {
                _Shots--;
                _Sfx.Play(Files.Sfx_Simpleshot);
                _EffectLight.Visible = true;
                _Core.AnimationManager.Wait(WeaponData.DrakePrimary.FireRate / 2, Shot);
            }
            else
            {
                _Core.AnimationManager.Wait(_Core.Random.Next(3, 8), StartAnimation);
            }
        }

        protected override void Destroy()
        {
            foreach (ServerInfo info in _ServerList.Components)
            {
                info.Checked -= HandleServerSelected;
            }

            _Core.DeviceResized -= HandleCoreDeviceResized;
            _Sfx = null;
        }
    }
}