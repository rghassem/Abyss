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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class RigidBlock : PhysicsObject
    {
        public RigidBlock(GameScreen screen, Vector2 pos, string sprt, ref World world, float width, float height, float rotation = 1)
            : base(screen, pos, sprt, ref world, width, height)
        {
			PhysicsBody.Body.Rotation = rotation;
			PhysicsBody.Body.Position = pos;
        }

		protected override void createBody(ref World world)
		{
			PhysicsBody = FixtureFactory.CreateRectangle(world, width, height, 1);
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
    }
}
