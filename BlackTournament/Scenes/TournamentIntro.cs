using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;
using BlackTournament.Controller;

namespace BlackTournament.Scenes
{
    public class TournamentIntro : Scene
    {
        private MenuController _MenuController;

        private float _Counter;
        private Graphic _BG;
        private Graphic _Title;
        private Graphic _Logo;
        private TextItem _Text;
        private Graphic _CenterT;


        public TournamentIntro(Core core, MenuController menuController) : base(core, nameof(TournamentIntro), Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
            _MenuController = menuController;
        }


        protected override bool Load()
        {
            // Title BG
            Layer_Game.Add(_BG = new Graphic(_Core, TextureLoader.Load(Files.Menue_Glow))
            {
                Alpha = 0.6f
            });

            // Title Text Graphic
            Layer_Game.Add(_Title = new Graphic(_Core, TextureLoader.Load(Files.Menue_Title)));
            _Title.Origin = new Vector2f(_Title.Texture.Size.X / 2, 0);

            // Logo
            Layer_Game.Add(_Logo = new Graphic(_Core, TextureLoader.Load(Files.Menue_Logo))
            {
                Origin = new Vector2f(345, 355)
            });

            // Loading Text
            Layer_Game.Add(_Text = new TextItem(_Core, "loading...", 16, Game.StyleFont)
            {
                Position = Create.Vector2f(30)
            });

            // Stationary "T" in Logo center
            Layer_Game.Add(_CenterT = new Graphic(_Core, TextureLoader.Load(Files.Menue_Title))
            {
                TextureRect = new IntRect(0, 90, 100, 100),
                Origin = new Vector2f(50, 25)
            });
            
            // Adapt to Resolution
            _Core.DeviceResized += HandleDeviceResized;
            HandleDeviceResized(_Core.DeviceSize);

            // Animation
            _Core.AnimationManager.Wait(_Core.Random.NextFloat(4, 6), () => _MenuController.Activate());
            return true;
        }

        private void HandleDeviceResized(Vector2f size)
        {
            // Update Scaling
            _Title.Scale = _Logo.Scale = _CenterT.Scale = Create.Vector2f(size.X < _Logo.Texture.Size.X * 2 ? 0.5f : 1);
            //Update Positions
            _BG.Position = new Vector2f(size.X / 2 - _BG.Texture.Size.X / 2, size.Y - _BG.Texture.Size.Y * 0.7f);
            _Title.Position = new Vector2f(size.X / 2, size.Y - _Title.GetGlobalBounds().Height - 30);
            _Logo.Position = new Vector2f(size.X / 2, (size.Y + (_Title.Position.Y - size.Y)) / 2);
            _CenterT.Position = _Logo.Position-new Vector2f(2,0);
        }

        protected override void Update(float deltaT)
        {
            _Logo.Rotation += 20 * deltaT;

            _Counter += deltaT;
            if (_Counter > 0.45f)
            {
                _Counter = 0;
                // Not pretty but gets the job done
                if (_Text.DisplayedString == "loading") _Text.DisplayedString = "loading.";
                else if (_Text.DisplayedString == "loading.") _Text.DisplayedString = "loading..";
                else if (_Text.DisplayedString == "loading..") _Text.DisplayedString = "loading...";
                else if (_Text.DisplayedString == "loading...") _Text.DisplayedString = "loading";
            }
        }

        protected override void Destroy()
        {
            _Core.DeviceResized -= HandleDeviceResized;
        }
    }
}