using System;
using System.Collections.Generic;

namespace BlackTournament
{
    public static class Files
    {
        // SFX
        public static readonly IReadOnlyCollection<String> MENUE_SFX = new[] { Sfx_Highlight, Sfx_Select };
        public static readonly IReadOnlyCollection<String> GAME_SFX = new[] { Sfx_Explosion, Sfx_Grenatelauncher, Sfx_LaserBlastSmall, Sfx_LaserBlastBig, Sfx_Pickup1, Sfx_Pickup2, Sfx_Pickup3, Sfx_Simpleshot, Sfx_Spark, Sfx_Pew };

        public const String Sfx_Explosion = "explosion";
        public const String Sfx_Grenatelauncher = "grenatelauncher";
        public const String Sfx_LaserBlastSmall = "laserblastsmall";
        public const String Sfx_LaserBlastBig = "laserblastbig";
        public const String Sfx_Pickup1 = "pickup1";
        public const String Sfx_Pickup2 = "pickup2";
        public const String Sfx_Pickup3 = "pickup3 long";
        public const String Sfx_Simpleshot = "simple shot";
        public const String Sfx_Highlight = "Highlight";
        public const String Sfx_Select = "Select";
        public const String Sfx_Spark = "spark";
        public const String Sfx_Pew = "pew";

        // MUSIC
        public static readonly IReadOnlyCollection<String> MENUE_MUSIC = new[] { Music_Invading_a_Submarine, Music_Ten_Seconds_to_Rush, Music_Third_Level_Encryption };
        public static readonly IReadOnlyCollection<String> GAME_MUSIC = new[] { Music_Industrial_Rage, Music_Its_Totally_and_Completely_Barbaric, Music_Teks_Abomination, Music_Venting_Your_Spleen_on_Low_B, Music_Wall_of_Anger };

        public const String Music_Industrial_Rage = "Industrial_Rage";
        public const String Music_Invading_a_Submarine = "Invading_a_Submarine";
        public const String Music_Its_Totally_and_Completely_Barbaric = "Its_Totally_and_Completely_Barbaric";
        public const String Music_Teks_Abomination = "Teks_Abomination";
        public const String Music_Ten_Seconds_to_Rush = "Ten_Seconds_to_Rush";
        public const String Music_Third_Level_Encryption = "Third_Level_Encryption";
        public const String Music_Venting_Your_Spleen_on_Low_B = "Venting_Your_Spleen_on_Low_B";
        public const String Music_Wall_of_Anger = "Wall_of_Anger";

        // EMITTER
        public const String Emitter_Smoke_Black = "Smoke_Black";
        public const String Emitter_Smoke_Grey = "Smoke_Grey";
        public const String Emitter_Smoke_White = "Smoke_White";
        public const String Emitter_Shockwave = "Shockwave";
    }
}