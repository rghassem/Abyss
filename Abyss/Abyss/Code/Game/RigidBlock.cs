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
        public RigidBlock(GameScreen screen, Vector2 pos, Texture2D sprt, ref World world, float width, float height)
            : base(screen, pos, sprt, ref world)
        {
            world.RemoveBody(PhysicsBody.Body);
            PhysicsBody = FixtureFactory.CreateRectangle(world, width, height, 1);
            PhysicsBody.Body.Position = pos;
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
    }
}
