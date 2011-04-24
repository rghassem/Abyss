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
using FarseerPhysics.Dynamics.Contacts;
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
		const int BUMP_OFF_ENEMY_FORCE = 50;

		const uint STARTING_HEALTH = 50;

		private uint health;
		public uint Health {
			get
			{
				return health;
			}
			set
			{
				if (!(health <= 0))
					health = value;
			}
		}

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

			Zindex = 0.4f;
			
			animationManager = new AnimationManager("Animations/lundSprite");
			loadSprite("Animations/lundSprite");

			//add animations
			animationManager.addAnimation("Run", WALK_CYCLE_LENGTH,
				"jog01", "jog02", "jog03", "jog04", "jog05", "jog06", "jog07", "jog08", "jog09", "jog10", "jog11", "jog12", "jog13");
			animationManager.addAnimation("Idle", 0.5f,
				"idle");

			//Init heath
			Health = STARTING_HEALTH;
        }

		protected override void createBody(ref World world)
		{
			base.createBody(ref world);
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
				playIdleAnim();
        }

		protected override void playMovementAnim()
		{
			base.playMovementAnim();
			animationManager.playAnim("Run");
		}

		protected override void playIdleAnim()
		{
			base.playMovementAnim();
			animationManager.playAnim("Idle");
		}

		protected override bool onCollision(Fixture f1, Fixture f2, Contact contact)
		{
			Fixture possibleGoblin = (f1  == PhysicsBody) ? f2 : f1;
			if (possibleGoblin.UserData is Goblin)
			{
				Vector2 pushBackImpulse;
				if(PhysicsBody.Body.LinearVelocity.X > 0)
					pushBackImpulse = new Vector2(BUMP_OFF_ENEMY_FORCE, PhysicsBody.Body.LinearVelocity.Y);
				else if (PhysicsBody.Body.LinearVelocity.X < 0)
					pushBackImpulse = new Vector2(-BUMP_OFF_ENEMY_FORCE, PhysicsBody.Body.LinearVelocity.Y);
				else //if 0, we're standing still
				{
					if (possibleGoblin.Body.LinearVelocity.X > 0)
						pushBackImpulse = new Vector2(BUMP_OFF_ENEMY_FORCE, PhysicsBody.Body.LinearVelocity.Y);
					else if (possibleGoblin.Body.LinearVelocity.X < 0)
						pushBackImpulse = new Vector2(-BUMP_OFF_ENEMY_FORCE, PhysicsBody.Body.LinearVelocity.Y);
					else //if somehow they are both standing still, leave them alone.
						pushBackImpulse = Vector2.Zero;
				}
				push(pushBackImpulse);
			}
			return base.onCollision(f1, f2, contact);
		}

		/// <summary>
		/// Deal damage to the player.
		/// </summary>
		/// <param name="damage">Amount to subtract from player's health.</param>
		/// <param name="impulse">Amount of force to push the player back by.</param>
		/// <returns>True if hit succeeds and will affect the player.</returns>
		public bool takeHit(GameObject hostileActor, uint damage, Vector2 impulse )
		{
			Health -= damage;
			push(impulse);
			return true;
		}

		public bool takeHit(PhysicsObject hostileActor, uint damage)
		{
			return takeHit(hostileActor, damage, Vector2.Zero);
		}
    }
}
