using System;

namespace OpenKh.Game
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new OpenKhGame(args))
                game.Run();
        }
    }
}
