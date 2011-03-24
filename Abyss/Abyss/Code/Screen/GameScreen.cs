using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Abyss.Code.Game;
using Abyss.Code.UserInterface.OSD;
using FarseerPhysics.Dynamics; 

namespace Abyss.Code.Screen 
{

    /// <summary>
    /// The Screen where all the gameplay happens. Contains a lot of GameObjects, and has a FarseerCamera which is used to
    /// display everything except the HUD (including the FaseerPhysics DebugView). GameScreen is a Singleton.
    /// </summary>
    public class GameScreen : Screen
    {
		public AbyssGame Game;
        private List<GameObject> GameObjects;
        public World world;
        public FarseerCamera Camera;
        public PlayerCharacter PC;

        private OSD osd;

        public const int PIXELS_PER_METER = 50;
        public const int GRAVITY = 20;

        public GameScreen(AbyssGame game)
        {
			Game = game;
            world = new World(new Vector2(0, GRAVITY));
            UnitConverter.SetDisplayUnitToSimUnitRatio(PIXELS_PER_METER);
			Camera = new FarseerCamera(ref AbyssGame.spriteBatch, AbyssGame.Assests, Game.GraphicsDevice, 
                Game.ScreenWidth, Game.ScreenHeight, ref world, PIXELS_PER_METER);
            Camera.viewMode = FarseerCamera.ViewMode.Scroll;
            
            GameObjects = new List<GameObject>(5); //probably a good starting number
        }

        /// <summary>
        /// This method will load a level from memory, and add to the GameObjects list accordingly.
        /// For now, I just manually set what I want in the level from here.
        /// </summary>
        public void loadLevel()
        {
			PC = new PlayerCharacter(this, new Vector2(15, 10), "Animations/lundSprite", ref world, 120, 120);
            GameObjects.Add(PC);
            GameObjects.Add(new RigidBlock(this, new Vector2(15, 12), null, ref world,
				50, 1, 0.0f));
			GameObjects.Add(new RigidBlock(this, new Vector2(20,8), null, ref world,
				50, 1, 100.0f));

            Camera.Subject = PC;
            Camera.subjetDistanceToScreenEdge = 700;

            osd = new OSD();
            osd.LoadContent();

			foreach (GameObject go in GameObjects)
				go.postLoadLevel();
        }

        public void update(GameTime gameTime)
        {
            //no need to update GameObjects, they're GameComponents, and will get the update automatically.
            world.Step(gameTime.ElapsedGameTime.Milliseconds * .001f);
            Camera.update();
        }

        public void draw(GameTime gameTime)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects.ElementAt<GameObject>(i).draw(gameTime);
            }

            Camera.draw();
			AbyssGame.spriteBatch.Begin();
                osd.Draw(gameTime);
			AbyssGame.spriteBatch.End();

        }
            
    }
}
