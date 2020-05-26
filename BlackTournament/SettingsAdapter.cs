using BlackTournament.Properties;
using BlackCoat;

namespace BlackTournament
{
    class SettingsAdapter : Launcher.ISettingsAdapter
    {
        public (uint X, uint Y) Resolution
        {
            get => (Settings.Default.ResolutionX, Settings.Default.ResolutionY);
            set { Settings.Default.ResolutionX = value.X; Settings.Default.ResolutionY = value.Y; }
        }
        public uint AntiAliasing { get => Settings.Default.AntiAliasing; set => Settings.Default.AntiAliasing = value; }
        public uint FpsLimit { get => Settings.Default.FpsLimit; set => Settings.Default.FpsLimit = value; }
        public bool Windowed { get => Settings.Default.Windowed; set => Settings.Default.Windowed = value; }
        public bool Borderless { get => Settings.Default.Borderless; set => Settings.Default.Borderless = value; }
        public bool VSync { get => Settings.Default.VSync; set => Settings.Default.VSync = value; }
        public int MusicVolume { get => Settings.Default.MusikVolume; set => Settings.Default.MusikVolume = value; }
        public int EffectVolume { get => Settings.Default.SfxVolume; set => Settings.Default.SfxVolume = value; }
    }
}