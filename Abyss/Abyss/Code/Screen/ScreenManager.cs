using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Abyss.Code.Screen
{
	/// <summary>
	/// Draws and updates the screens. Stores screens in a stack, though currently this
	/// is not really being used and its just a wrapper around a GameScreen. This will need
	/// to be changed when we implement menus and inventories and such.
	/// </summary>
    public class ScreenManager
    {
		AbyssGame Game;
		GameScreen gameScreen; // this should probably not be here, instead just use the stack
        Stack<Screen> ScreenStack;

        public ScreenManager(AbyssGame g)
        {
			Game = g;
			gameScreen = new GameScreen(Game);
            ScreenStack = new Stack<Screen>();
			ScreenStack.Push(gameScreen);
        }

        public void update(GameTime gameTime)
        {
            gameScreen.update(gameTime);
        }

		public void loadGameLevel()
		{
			gameScreen.loadLevel();
		}

        public void drawActiveScreen(GameTime gameTime)
        {
			ScreenStack.Peek().draw(gameTime);
            //gameScreen.draw(gameTime);
        }
    }
}
