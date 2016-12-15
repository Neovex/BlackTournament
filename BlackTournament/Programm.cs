using System;
using System.Linq;


namespace BlackTournament
{
    public static class Programm
    {
        public static void Main(string[] args)
        {
            var blackTournament = new Game();
            // TODO : Master try catch (see TF3 & co)
            blackTournament.Run(String.Join(" ", args ?? Enumerable.Empty<String>()));
            blackTournament = null;
        }
    }
}