using System;
using System.Linq;


namespace BlackTournament
{
    public static class Programm
    {
        public static void Main(string[] args)
        {
            var blackCoat = new Game();
            // TODO : Master try catch (see TF3 & co)
            blackCoat.Run(String.Join(" ", args ?? Enumerable.Empty<String>()));
            blackCoat = null;
        }
    }
}