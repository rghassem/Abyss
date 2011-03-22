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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Abyss.Code.Screen;


namespace Abyss.Code.Game
{
    /// <summary>
    /// The is a GameObject which has physics. Physics done via Farseer Physics Engine 3.1.
    /// </summary>
    public abstract class PhysicsObject : GameObject
    {

        protected Fixture PhysicsBody;

        public override Vector2 Position 
        {
            get 
            {
                if (PhysicsBody != null)
                    return PhysicsBody.Body.Position;
                else return position; 
            } 
            set 
            {
                if (PhysicsBody != null )
                    PhysicsBody.Body.Position = value;
                else position = value;
            }
        }

        public PhysicsObject(GameScreen screen, Vector2 pos, Texture2D sprt, ref World world)
            : base(screen, pos, sprt)
        {
            createBody(ref world);
            // TODO: Construct any child components here
        }

        protected virtual void createBody(ref World world)
        {
            float BodyHeight = (Sprite != null) ? UnitConverter.ToSimUnits(Sprite.Height / 2) : 1;
            PhysicsBody = FixtureFactory.CreateCircle(world, BodyHeight, 1);
            PhysicsBody.Body.Position = position;
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

        public override void draw(GameTime gameTime)
        {
            environment.Camera.record(this);
        }
    }
}
