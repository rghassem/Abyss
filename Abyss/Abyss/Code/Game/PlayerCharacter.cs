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
using Abyss.Code.Screen;


namespace Abyss.Code.Game
{
    /// <summary>
    /// The player character, reacts to input from user. (Can rename this to "Lund" if we decide to stick with that name.)
    /// </summary>
    public class PlayerCharacter : Character
    {
        const float MAX_HORIZONTAL_SPEED = 10;
		float movementAccel = 10;

        public PlayerCharacter(GameScreen screen , Vector2 pos, Texture2D sprt, ref World world)
			: base(screen, pos, sprt, ref world)
        {
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
            handlePlayerInput();
            base.Update(gameTime);
        }

        /// <summary>
        /// Where input from the player is handled. Lots to do here, needs to only work if 
		/// GameScreen is active, needs to determine when we are
        /// standing on something in a less hacky manner. Numbers need to be fine tuned, and
        /// I think there is a bug in the movement code.
        /// </summary>
        private void handlePlayerInput()
        {
            Vector2 impulse = Vector2.Zero;

            if (Input.isRightHeld())
                impulse.X += movementAccel;

			else if (Input.isLeftHeld())
                impulse.X -= movementAccel;
            else if (!Input.isLeftHeld() && !Input.isRightHeld())
                PhysicsBody.Body.LinearVelocity = new Vector2(0, PhysicsBody.Body.LinearVelocity.Y);

            if (Input.jumpPressed())
            {
                if (PhysicsBody.Body.LinearVelocity.Y <= 0.01 && PhysicsBody.Body.LinearVelocity.Y >= -0.01)
                {
                    impulse.Y -= 30;
                }
            }

            PhysicsBody.Body.ApplyLinearImpulse(ref impulse);

            //limit horizontal speed
            if (Math.Abs(PhysicsBody.Body.LinearVelocity.X) > MAX_HORIZONTAL_SPEED)
            {
                Vector2 lv = PhysicsBody.Body.LinearVelocity;
                float lvx = Math.Abs(lv.X);
                float diff = (lvx - MAX_HORIZONTAL_SPEED) * (lv.X / lvx);
                Vector2 newVelocity = new Vector2(lv.X - diff, lv.Y);

                PhysicsBody.Body.LinearVelocity = newVelocity;
            }
        }

    }
}
