using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using BlackCoat;
using BlackCoat.UI;
using BlackCoat.Animation;
using SFML.Graphics;

using BlackTournament.Net.Data;
using BlackTournament.InputMaps;

namespace BlackTournament.UI
{
    class HUD : Canvas
    {
        private readonly static Color _GOLD = new Color(255, 215, 0);

        // Game
        public int Score { set => _ScoreLabel.Text = value.ToString(); }
        public int Rank
        {
            set
            {
                _RankLabel.Text = value.ToString();
                _RankLabel.TextColor = value == 1 ? _GOLD : Color.White;
            }
        }
        public int TotalPlayers { set => _TotalLabel.Text = $"-{value}"; }
        public TimeSpan Time { set => _TimeLabel.Text = value.ToString("mm\\:ss"); }

        // Player
        public float Health
        {
            set
            {
                if (value < 1 && value != 0) value = 1;
                _HealthLabel.Text = value.ToString("0");
                _HealthLabel.TextColor = value < 20 ? Color.Red : Color.White;
            }
        }
        public float Shield
        {
            set
            {
                if (value < 1 && value != 0) value = 1;
                _ShieldInactive.Visible = value == 0;
                _ShieldActive.Visible = !_ShieldInactive.Visible;
                _ShieldLabel.Text = value.ToString("0");
                _ShieldLabel.TextColor = value < 20 && value != 0 ? Color.Red : Color.White;
            }
        }
        public bool Alive
        {
            set
            {
                _PlayerInfo.Visible = value;
                _Weapons.Visible = value;
            }
        }

        public override View View { get => null; set => base.View = value; } // Disable view inheritance
        public String ChatMessage => _ChatInputTextBox.Text;
        public ScoreBoard ScoreBoard { get; private set; }
        public InGameMenu Menu { get; private set; }

        private Dictionary<PickupType, WeaponIcon> _WeaponLookup;
        private AlignedContainer _PlayerInfo;
        private AlignedContainer _Weapons;
        private Label _ScoreLabel;
        private Label _TimeLabel;
        private Label _RankLabel;
        private Label _TotalLabel;
        private Label _HealthLabel;
        private Label _ShieldLabel;
        private UIGraphic _ShieldActive;
        private UIGraphic _ShieldInactive;
        private OffsetContainer _ChatContainer;
        private TextBox _ChatInputTextBox;

        public HUD(Core core, TextureLoader texLoader, SfxManager sfx, UIInputMap input) : base(core, core?.DeviceSize)
        {
            // Build HUD
            // Score and Time
            var snt = new AlignedContainer(_Core, Alignment.CenterTop,
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_TopBg)) { Name = "TOP BG" },
                    new OffsetContainer(_Core, Orientation.Horizontal, 10,
                            _ScoreLabel = new Label(_Core, "0", 40, Game.DefaultFont)
                            {
                                Name = "Score",
                                Margin = (0, 16, 0, 0),
                                Padding = (0, 0, -10, 0) // Texts are weird
                            },
                            new UIGraphic(_Core, texLoader.Load(Files.HUD_Skull)) { Name = "Skull" }
                        )
                    {
                        Position = (10, 0)
                    },
                    new AlignedContainer(_Core, Alignment.CenterTop, _TimeLabel = new Label(_Core, "TIME", 20, Game.DefaultFont)
                    {
                        Name = "Time",
                        Position = (0, 25)
                    }),
                    new AlignedContainer(_Core, Alignment.TopRight,
                            _RankLabel = new Label(_Core, "1", 40, Game.DefaultFont)
                            {
                                Name = "Rank",
                                Position = (0, 16),
                                Alignment = TextAlignment.Right
                            },
                            _TotalLabel = new Label(_Core, "-10", 20, Game.DefaultFont)
                            {
                                Name = "Total",
                                Position = (6, 32),
                                Padding = (0, 0, 10, 0)
                            }
                        )
            )
            {
                Name = "Game"
            };
            Add(snt);

            // Health and Shield
            _PlayerInfo = new AlignedContainer(_Core, Alignment.CenterBottom,
                new Canvas(_Core, (200, 36),
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_Health))
                    {
                        Name = "Health Icon",
                        Position = (3, 2)
                    },
                    _HealthLabel = new Label(_Core, "100", 26, Game.DefaultFont)
                    {
                        Name = "Health",
                        Position = (45, 7)
                    },
                    new AlignedContainer(_Core, Alignment.TopRight,
                        _ShieldLabel = new Label(_Core, "100", 26, Game.DefaultFont)
                        {
                            Name = "Shield"
                        }
                    )
                    {
                        Margin = (0, 7, 45, 0)
                    },
                    _ShieldActive = new UIGraphic(_Core, texLoader.Load(Files.HUD_ShieldActive))
                    {
                        Name = "Shield Active Icon",
                        Position = (165, 1),
                        Visible = false
                    },
                    _ShieldInactive = new UIGraphic(_Core, texLoader.Load(Files.HUD_ShieldInactive))
                    {
                        Name = "Shield Inactive Icon",
                        Position = _ShieldActive.Position,
                        Visible = true
                    }
                )
            )
            {
                Name = "Player",
                BackgroundColor = Color.Black,
                BackgroundAlpha = 0.35f,
                Margin = (0, 0, 0, 60)
            };
            Add(_PlayerInfo);

            // Weapons
            var drake = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Drake_White), texLoader.Load(Files.HUD_Drake_Outline));
            var hedge = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Hedgeshock_White), texLoader.Load(Files.HUD_Hedgeshock_Outline));
            var thump = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Thumper_White), texLoader.Load(Files.HUD_Thumper_Outline));
            var titan = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Titandrill_White), texLoader.Load(Files.HUD_Titandrill_Outline));

            _Weapons = new AlignedContainer(_Core, Alignment.CenterBottom,
                new OffsetContainer(_Core, Orientation.Horizontal, 20, drake, hedge, thump, titan)
            )
            {
                Name = "Weapons",
                Margin = (0, 0, 0, 12)
            };
            Add(_Weapons);

            _WeaponLookup = new Dictionary<PickupType, WeaponIcon>()
            {
                { PickupType.Drake, drake },
                { PickupType.Hedgeshock, hedge },
                { PickupType.Thumper, thump },
                { PickupType.Titandrill, titan }
            };

            // Message / Chat System
            var chat = new AlignedContainer(_Core, Alignment.BottomLeft,
                _ChatContainer = new OffsetContainer(_Core, Orientation.Vertical, 6)
            )
            {
                Name = "Chat",
                Margin = (20, 0, 0, 60)
            };
            _ChatInputTextBox = new TextBox(_Core, new Vector2f(205, 19), 16, Game.DefaultFont)
            {
                Input = input,
                Name = "InputBox",
                Padding = new FloatRect(5, 5, 0, 0),
                BackgroundColor = Color.Black,
                BackgroundAlpha = 0.5f,
                CaretColor = Color.White,
                EditingTextColor = Color.White,
                EditingBackgroundColor = new Color(0, 0, 0, 50)
            };
            Add(chat);

            // Score Overlay
            ScoreBoard = new ScoreBoard(_Core);
            Add(ScoreBoard);

            // Menu
            Menu = new InGameMenu(_Core, texLoader, sfx)
            {
                Input = input
            };
            Add(Menu);

            // Listen to Size Changes
            _Core.DeviceResized += Resize;
        }

        internal void SetPlayerWeapons(PickupType currentWeapon, IEnumerable<Weapon> weapons)
        {
            foreach (var weaponIcon in _WeaponLookup.Values)
            {
                weaponIcon.UpdateIcon(false, 0, 0);
            }
            foreach (var weaponData in weapons)
            {
                _WeaponLookup[weaponData.WeaponType].UpdateIcon(weaponData.WeaponType == currentWeapon, weaponData.PrimaryAmmo, weaponData.SecundaryAmmo);
            }
        }

        internal void EnableChat()
        {
            _ChatContainer.Add(_ChatInputTextBox);
            _ChatInputTextBox.Text = String.Empty;
            _ChatInputTextBox.GiveFocus();
        }
        internal void DisableChat()
        {
            if (_ChatContainer.Contains(_ChatInputTextBox))
            {
                _ChatContainer.Remove(_ChatInputTextBox);
            }
        }

        internal void ShowMessage(bool isSystemMessage, string message)
        {
            // Create Message Label
            var msgLabel = new Label(_Core, message, 16, Game.DefaultFont)
            {
                Padding = new FloatRect(5, 5, 5, 5),
                TextColor = Color.White,
                BackgroundColor = Color.Black,
                BackgroundAlpha = 0.5f
            };
            if (isSystemMessage)
            {
                msgLabel.Style = Text.Styles.Bold;
                msgLabel.TextColor = Color.Red;
            }
            _ChatContainer.Add(msgLabel);

            // Hard limit to prevent Spam
            if (_ChatContainer.Count > 10)
            {
                _ChatContainer.GetFirst<Label>().Dispose();
            }

            // Soft Limit via fade out
            _Core.AnimationManager.Run(1, 0, 8, v =>
            {
                if (!msgLabel.Disposed) msgLabel.Alpha = v;
            }, msgLabel.Dispose, InterpolationType.InExpo);

            // Handle Input Box
            if (_ChatContainer.Contains(_ChatInputTextBox))
            {
                _ChatContainer.Remove(_ChatInputTextBox);
                _ChatContainer.Add(_ChatInputTextBox);
            }
        }

        protected override void Destroy(bool disposing)
        {
            _Core.DeviceResized -= Resize;
            base.Destroy(disposing);
        }
    }
}