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
        public World theWorld;
        public FarseerCamera Camera;
        public PlayerCharacter PC;

        private OSD osd;

        public const int PIXELS_PER_METER = 50;
        public const int GRAVITY = 20;

        public GameScreen(AbyssGame game)
        {
			Game = game;
            theWorld = new World(new Vector2(0, GRAVITY));
            UnitConverter.SetDisplayUnitToSimUnitRatio(PIXELS_PER_METER);
			Camera = new FarseerCamera(ref AbyssGame.spriteBatch, AbyssGame.Assests, Game.GraphicsDevice, 
                Game.ScreenWidth, Game.ScreenHeight, ref theWorld, PIXELS_PER_METER);
            Camera.viewMode = FarseerCamera.ViewMode.Scroll;
            
            GameObjects = new List<GameObject>(5); //probably a good starting number
        }

        /// <summary>
        /// This method will load a level from memory, and add to the GameObjects list accordingly.
        /// For now, I just manually set what I want in the level from here.
        /// </summary>
        public void loadLevel()
        {
			Texture2D PlayerSprite = AbyssGame.Assests.Load<Texture2D>("lund_idle_down_scaled_2");
            PC = new PlayerCharacter(this, new Vector2(15, 5), PlayerSprite, ref theWorld);
            GameObjects.Add(PC);
            GameObjects.Add(new RigidBlock(this, new Vector2(15, 12), null, ref theWorld,
                50, 1));

            Camera.Subject = PC;
            Camera.subjetDistanceToScreenEdge = 500;

            osd = new OSD();
            osd.LoadContent();
        }

        public void update(GameTime gameTime)
        {
            //no need to update GameObjects, they're GameComponents, and will get the update automatically.
            theWorld.Step(gameTime.ElapsedGameTime.Milliseconds * .001f);
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
