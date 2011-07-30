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
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Abyss.Code.Screen;

namespace Abyss.Code.Game
{
    /// <summary>
    /// A Character in the Game. Extends PhysicsObjects because characters also have physics.
    /// </summary>
    public class Character : PhysicsObject
    {
		//Movement Variables
		const double GROUND_NORMAL_Y_LIMIT = -0.2;
		protected float JUMP_IMPULSE_TIME = 1f;
		protected float JUMPING_ANIM_TIME = 0.2f;

		protected Color DrawColor = Color.White;

		protected float MaxSpeed = 8;
		protected float MaxAirSpeed = 12;
		protected float JumpHeight = 20;
		protected float LongJumpBonus = 15;
		protected float MovementAccel = 10;
		protected float AirAccel = 2;
		protected float stepTime = 0.1f;

		protected bool FacingLeft { get; set; }

		protected bool inStep;
		private float timeSinceStep;

		private double prevAngleOfRotation = 0.0f;

		//Private Jump Variables
		private bool moveLeft;
		private bool moveRight;
		protected bool jump;
		private bool longJump;
		private bool jumping;
		private bool pushed = false;

		private float timeTillEndJump;
		private float timeTillEndJumpAnim;
		private float timeTillEndJumpPrep;

		bool wallSlopeUnderLimit;

		//Particle effects
		private ParticleEntity dustKickUpParticle;

		protected Vector2 groundVector;
		protected Vector2 groundNormal = Vector2.UnitY;
		protected float groundSlope;
		float slopeLimit = 1.3f;
		protected float groundStickiness = 3f;
		HashSet<Fixture> ground = new HashSet<Fixture>();
		protected bool onGround
		{
			get
			{
				if (jumping)
					return false;
				return ground.Count > 0;		
			}
			set { } //can't be set, just is true if we currently have a ground.
		}

		public Character(GameScreen screen, float width, float height)
			: base(screen, width, height)
		{
			PhysicsBody.Body.BodyType = BodyType.Dynamic;

			PhysicsBody.OnCollision += onCollision;
			PhysicsBody.OnSeparation += onSeperation;

			//initialize the particle effect
			dustKickUpParticle = new ParticleEntity(screen, "Dust");
			screen.addObject(dustKickUpParticle);
			timeTillEndJumpAnim = 0;
			timeTillEndJumpPrep = 0;
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

		protected override void createBody(ref World world)
		{
			float BodyHeight = (Sprite != null) ? UnitConverter.ToSimUnits(height / 2) : 1;
			PhysicsBody = FixtureFactory.CreateCircle(world, BodyHeight, 1);
			PhysicsBody.Body.Position = base.Position;
			PhysicsBody.Body.FixedRotation = true;
		}

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
			updateMovement(gameTime);
			animationManager.animate(gameTime);

            base.Update(gameTime);
        }

//Movement code:
		protected virtual void stepRight()
		{
			if (!pushed)
			{
				moveRight = true;
				moveLeft = false;
				if (onGround)
					FacingLeft = false;
				inStep = true;
				timeSinceStep = 0;
			}
		}

		protected virtual void stepLeft()
		{
			if (!pushed)
			{
				moveLeft = true;
				moveRight = false;
				if (onGround)
					FacingLeft = true;
				inStep = true;
				timeSinceStep = 0;
			}
		}

		/// <summary>
		/// Pushes the character by the given impulse, disregarding other movement.
		/// </summary>
		/// <param name="impulse"></param>
		protected void push(Vector2 impulse)
		{
			moveRight = false;
			moveLeft = false;
			PhysicsBody.Body.ApplyLinearImpulse(ref impulse);
			pushed = true;
		}

		protected void changeDirection()
		{
			FacingLeft = !FacingLeft;
		}

		protected virtual void updateStep(GameTime gameTime)
		{
			timeSinceStep += gameTime.ElapsedGameTime.Milliseconds*0.001f;

			//emit dust particle effect when walking
			if (onGround)
				dustKickUpParticle.Effect.Trigger(UnitConverter.ToDisplayUnits(Position)
					+ new Vector2(0, animationManager.CurrentFrame.Height / 2));

			if (timeSinceStep >= stepTime)
			{
				moveLeft = false;
				moveRight = false;
				inStep = false;
			}
		}

		private void updateMovement(GameTime gameTime)
		{
			updateRotation();

			if (inStep)
				updateStep(gameTime);

			//TODO: Convert normal vector into a rotatioin in degrees
			Vector2 impulse = Vector2.Zero;
			
			if (onGround)
			{
				groundVector = getGroundVector();
				groundSlope = groundVector.Y / groundVector.X;
				pushed = false;
			}

			wallSlopeUnderLimit = Math.Abs(groundSlope) < slopeLimit;

			if (moveRight && wallSlopeUnderLimit)
			{
				if (onGround)
				{
					impulse += groundVector * MovementAccel;
					playMovementAnim();
				}
				else
					impulse += Vector2.UnitX * AirAccel;
			}

			else if (moveLeft && wallSlopeUnderLimit)
			{
				if (onGround)
				{
					impulse += groundVector * -MovementAccel;
					playMovementAnim();
				}
				else
					impulse += Vector2.UnitX * -AirAccel;
			}

			/* JUMPING:
			 * 1.) Player has pressed jump button, if we are on ground, start the jump animation, and enter the 
			 * START_JUMP_DELAY.
			 * 2.) START_JUMP_DELAY should last just long enough to play the crouching part of the jump animation.
			 * 3.) Then start applying jump impulse, this puts us in the air.
			 */
			
			if (jump && onGround) 
			{
				longJump = (moveLeft || moveRight);
				timeTillEndJump = JUMP_IMPULSE_TIME; //time to apply upward impulse
				timeTillEndJumpAnim = JUMPING_ANIM_TIME;
			}

			//play the start jump animation.
			if (timeTillEndJumpAnim > 0)
			{
				playJumpAnim();
				timeTillEndJumpAnim -= gameTime.ElapsedGameTime.Milliseconds * .01f;
				if (timeTillEndJumpAnim <= 0) //when we finish...
				{
					jumping = true; //actually jump now
					timeTillEndJumpAnim = 0;
				}
			}
			else if (!inStep && onGround)
				playIdleAnim();
			else if(!onGround) 
				playJumpingAnim(); //otherwise we must be in the air and not doing a jump prep.

			//apply the upward impulse that starts off a jump.
			if (jumping)
			{
				inStep = false;
				float jumpHeight = JumpHeight;
				if (longJump)
					jumpHeight += LongJumpBonus;

				if (timeTillEndJump <= 0)
					jumping = false;
				else //jump
				{
					impulse -= Vector2.UnitY * (jumpHeight * (gameTime.ElapsedGameTime.Milliseconds * 0.01f));
					timeTillEndJump -= gameTime.ElapsedGameTime.Milliseconds * .01f;
				}
			}

			//stick to the ground a little
			if (onGround)
				impulse += -groundNormal * groundStickiness;

			PhysicsBody.Body.ApplyLinearImpulse(ref impulse);

			//limit speed
			if (onGround)
			{
				if (Math.Abs(PhysicsBody.Body.LinearVelocity.Length()) > MaxSpeed)
				{
					Vector2 adjustedVelocity = Vector2.Normalize(PhysicsBody.Body.LinearVelocity)
						* MaxSpeed;
					PhysicsBody.Body.LinearVelocity = adjustedVelocity;
				}
			}
			else //if in the air, apply the MAX_AIR_SPEED instead.
			{
				if (Math.Abs(PhysicsBody.Body.LinearVelocity.Length()) > MaxAirSpeed)
				{
					Vector2 adjustedVelocity = Vector2.Normalize(PhysicsBody.Body.LinearVelocity)
						* MaxAirSpeed;
					PhysicsBody.Body.LinearVelocity = adjustedVelocity;
				}
			}

			if (!moveLeft && !moveRight && onGround && wallSlopeUnderLimit)
			{
				PhysicsBody.Body.LinearVelocity = new Vector2(0, 0);
			}

			//stop any rotation
			PhysicsBody.Body.AngularVelocity = 0;

			//ignore gravity if we are on the ground and not moving, that way no
			//sliding down hills.
			//TODO: Special cases will need to handled, eg what if we're being pushed back.
			PhysicsBody.Body.IgnoreGravity = (!moveLeft && !moveRight && onGround);

			//reset the controller jump boolean
			jump = false;

		}

		private Vector2 getGroundVector()
		{
			return new Vector2(-groundNormal.Y, groundNormal.X);
		}

		protected bool isGround(WorldManifold manifold, Fixture maybeGround)
		{
			if (maybeGround.UserData is PhysicsObject) //if its a physics object, its not the ground
				return false;
			if (manifold.Normal.Y > GROUND_NORMAL_Y_LIMIT) //which is -0.2
				return false;
			return manifold.Points[0].Y < (getFeetPosition().Y);// ||
					//manifold.Points[1].Y < (PhysicsBody.Body.Position.Y + PhysicsBody.Shape.Radius);
		}

		protected virtual bool onCollision(Fixture f1, Fixture f2, Contact contact)
		{
			Fixture obstacle = (f1  == PhysicsBody) ? f2 : f1;

			//if beneath the feet, treat it as ground
			WorldManifold manifold;
			contact.GetWorldManifold(out manifold);
			if (contact.IsTouching())
			{
				if (isGround(manifold, obstacle))
				{
					ground.Add(obstacle);
					groundNormal = manifold.Normal;
					groundVector = getGroundVector();
					groundSlope = groundVector.Y / groundVector.X;
					//Console.Out.WriteLine("OnCollision called");
				}
			}
			return true;
		}


		private void onSeperation(Fixture f1, Fixture f2)
		{
			Fixture obstacle = (f1 == PhysicsBody) ? f2 : f1;

			ground.Remove(obstacle);
			//Console.Out.WriteLine("OnSeperation called");
		}

		private void updateRotation()
		{
			if (onGround)
			{
				double angleOfRotation = Math.Acos(Vector2.Dot(Vector2.UnitX, groundNormal)) - (Math.PI/2);
				if (Math.Abs(angleOfRotation) < Math.PI/4) //don't rotate the image if the angle is steeper than 45d.
				{
					Rotation = ((float)-((angleOfRotation + prevAngleOfRotation) / 2));
					prevAngleOfRotation = angleOfRotation;
				}
			}
			else Rotation = 0;
		}

		protected virtual Vector2 getFeetPosition()
		{
			return PhysicsBody.Body.Position + new Vector2(0, PhysicsBody.Shape.Radius);
		}

		protected virtual Vector2 getEyePosition()
		{
			return PhysicsBody.Body.Position - new Vector2(0, (float)(PhysicsBody.Shape.Radius*0.75));
		}

		protected virtual void playMovementAnim() { }

		protected virtual void playIdleAnim() { }

		protected virtual void playJumpAnim() { }

		protected virtual void playJumpingAnim() { }

		public override void draw(GameTime gameTime)
		{
			Vector2 origin = new Vector2(animationManager.CurrentFrame.Width / 2, animationManager.CurrentFrame.Height / 2);
			//this just means mirror the sprite if we're not FacingLeft.
			SpriteEffects direction = (FacingLeft) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			environment.Camera.record(Sprite, Position, DrawColor, animationManager.CurrentFrame, Rotation, origin, new Vector2(Scale, Scale),
				direction, 0);
		}

		protected bool testSeeCharacter(Character targetChar)
		{
			Fixture nearestObjectInLine;
			testLineOfSight(targetChar.getEyePosition(), getEyePosition(), out nearestObjectInLine);
			return !(nearestObjectInLine != targetChar.PhysicsBody);//something obstructs our view of the target
		}


    }
}
