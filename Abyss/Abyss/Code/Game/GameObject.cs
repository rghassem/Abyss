using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Abyss.Code.Screen;


namespace Abyss.Code.Game
{
    /// <summary>
    /// An object that appears in Abyss gameplay, eg a character, landscape, obstacle, explosion effect, etc.
    /// A game component that implements IUpdateable.
    /// </summary>
    public abstract class GameObject : GameComponent
    {
		public float Rotation;
		public float Scale;
		public Texture2D Sprite { get; private set; }
        public virtual Vector2 Position 
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
		protected Vector2 position;

		protected GameScreen environment;
		protected AnimationManager animationManager;
		protected string spriteName;

        public GameObject(GameScreen screen, Vector2 pos, string spriteAssetName)
			: base(screen.Game)
        {
			spriteName = spriteAssetName;
			environment = screen;
            Position = pos;
			if (spriteAssetName != null)
			{
				string dummy = AbyssGame.Assests.RootDirectory;
				Sprite = AbyssGame.Assests.Load<Texture2D>(spriteAssetName);
				//make dummy animation manager assuming no animations, for now.
				animationManager = new AnimationManager(Sprite.Width, Sprite.Height);
			}
			Rotation = 0;
			Scale = 1;
			environment.Game.addComponent(this);
        }

		/// <summary>
		/// Called as the last step after all other initialization.
		/// Physics objects contruct their Physics Bodies here.
		/// </summary>
		public virtual void postLoadLevel()
		{ 
		}

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the object using AbyssGame's spriteBatch. Must be called between 
        /// SpriteBatch.Begin() and SpriteBatch.End()
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void draw(GameTime gameTime)
        {
        }
    }
}
