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
		//Animation Constants
		float WALK_CYCLE_LENGTH = 0.5f;

		const int COLLISION_WIDTH = 96;
		const int COLLISION_HEIGHT = 96;

		public PlayerCharacter(GameScreen screen, SpawnPoint spawn)
			: this(screen) {
			Position = UnitConverter.ToSimUnits(spawn.Position);
		}

		public PlayerCharacter(GameScreen screen)
			: base(screen, COLLISION_WIDTH, COLLISION_HEIGHT)
        {
			screen.PC = this;
			screen.Camera.Subject = this;

			MaxSpeed = 5;
			MaxAirSpeed = 9;
			JumpHeight = 50;
			LongJumpBonus = 15;
			MovementAccel = 10;
			AirAccel = 2;

			//set scale
			Scale = 0.8f;
			
			animationManager = new AnimationManager("Animations/lundSprite");
			loadSprite("Animations/lundSprite");

			//add animations
			animationManager.addAnimation("Run", WALK_CYCLE_LENGTH,
				"jog01", "jog02", "jog03", "jog04", "jog05", "jog06", "jog07", "jog08", "jog09", "jog10", "jog11", "jog12", "jog13");
			animationManager.addAnimation("Idle", 0.5f,
				"idle");
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
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
        /// Where input from the player is handled.
        /// </summary>
        private void handlePlayerInput()
        {
			if (Input.isRightHeld())
			{
				stepRight();
				if(onGround)
					animationManager.playAnim("Run");
			}

			if (Input.isLeftHeld())
			{
				stepLeft();
				if(onGround)
					animationManager.playAnim("Run");
			}
     
            if (Input.jumpPressed())
            {
				jump = true;
				animationManager.playAnim("Idle");
            }

			if (!inStep)
				animationManager.loopAnim("Idle");
        }

    }
}
