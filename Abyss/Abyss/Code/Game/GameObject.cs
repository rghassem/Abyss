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
		protected GameScreen environment;
        protected Vector2 position;
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
        public Texture2D Sprite { get; private set; }

        public GameObject(GameScreen screen, Vector2 pos, Texture2D sprt)
			: base(screen.Game)
        { 
			environment = screen;
            Position = pos;
            if(sprt != null)
                Sprite = sprt;
			environment.Game.addComponent(this);
            // TODO: Construct any child components here
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
