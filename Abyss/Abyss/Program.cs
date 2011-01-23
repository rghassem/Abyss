using System;

namespace Abyss
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (AbyssGame game = new AbyssGame())
            {
                game.Run();
            }
        }
    }
#endif
}

