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
		public float Rotation = 0.0f;
		public float Scale = 1.0f;
		public float Zindex = 0.5f;

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
		private Vector2 position;

		protected GameScreen environment;
		protected AnimationManager animationManager;
		protected string spriteName;

		public GameObject(GameScreen screen)
				: base(screen.Game) {
			environment = screen;
		}

		protected override void Dispose(bool disposing) {
			environment.removeObject(this);
			base.Dispose(disposing);
		}

		public void loadSprite(string spriteAssetName) {
			spriteName = spriteAssetName;

			if (spriteAssetName != null)
			{
				string dummy = AbyssGame.Assets.RootDirectory;
				Sprite = AbyssGame.Assets.Load<Texture2D>(spriteAssetName);
				//make dummy animation manager assuming no animations, for now.
				if (animationManager == null) {
					animationManager = new AnimationManager(Sprite.Width, Sprite.Height);
				}
			}
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
