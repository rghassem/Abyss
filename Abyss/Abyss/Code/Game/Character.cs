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
		protected float MaxSpeed = 8;
		protected float MaxAirSpeed = 12;
		protected float JumpHeight = 30;
		protected float LongJumpBonus = 15;
		protected float MovementAccel = 10;
		protected float AirAccel = 2;
		protected float stepTime = 0.1f;

		protected bool FacingLeft { get; set; }

		protected bool inStep;
		private float timeSinceStep;

		protected bool moveLeft;
		protected bool moveRight;
		protected bool jump;
		protected bool longJump;
		bool jumping;

		bool wallSlopeUnderLimit;

		float timeSinceJump;
		Vector2 groundVector;
		Vector2 groundNormal = Vector2.UnitY;
		float groundSlope;
		float slopeLimit = 1f;
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

		public Character(GameScreen screen, Vector2 pos, string sprt, ref World world, float width, float height)
			: base(screen, pos, sprt, ref world, width, height)
        {
            // TODO: Construct any child components here
            
			animationManager = new AnimationManager(spriteName);
        }

		public override void postLoadLevel()
		{
			base.postLoadLevel();
			PhysicsBody.Body.BodyType = BodyType.Dynamic;

			PhysicsBody.OnCollision += onCollision;
			PhysicsBody.OnSeparation += onSeperation;
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
			PhysicsBody.Body.Position = position;
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
			moveRight = true;
			moveLeft = false;
			if (onGround)
				FacingLeft = false;
			inStep = true;
			timeSinceStep = 0;
		}

		protected virtual void stepLeft()
		{
			moveLeft = true;
			moveRight = false;
			if (onGround)
				FacingLeft = true;
			inStep = true;
			timeSinceStep = 0;
		}

		private void updateStep(GameTime gameTime)
		{
			timeSinceStep += gameTime.ElapsedGameTime.Milliseconds*0.001f;
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
			}

			wallSlopeUnderLimit = Math.Abs(groundSlope) < slopeLimit;

			if (moveRight && wallSlopeUnderLimit)
			{
				if (onGround)
				{
					impulse += groundVector * MovementAccel;
				}
				else
					impulse += Vector2.UnitX * AirAccel;
			}

			else if (moveLeft && wallSlopeUnderLimit)
			{
				if (onGround)
				{
					impulse += groundVector * -MovementAccel;
				}
				else
					impulse += Vector2.UnitX * -AirAccel;
			}

			if (!moveLeft && !moveRight && onGround && wallSlopeUnderLimit)
			{
				PhysicsBody.Body.LinearVelocity = new Vector2(0, 0);
			}
			
			if (jump && onGround) 
			{
				jumping = true;
				longJump = (moveLeft || moveRight) ? true : false;
				timeSinceJump = 0.8f; //time to apply upward impulse
			}

			if (jumping)
			{
				float jumpHeight = JumpHeight;
				if (longJump)
					jumpHeight += LongJumpBonus;

				if (timeSinceJump <= 0)
					jumping = false;
				else //jump
				{
					impulse -= Vector2.UnitY * (jumpHeight * (gameTime.ElapsedGameTime.Milliseconds * 0.01f));
					timeSinceJump -= gameTime.ElapsedGameTime.Milliseconds * .01f;
				}
			}

			PhysicsBody.Body.ApplyLinearImpulse(ref impulse);
			Console.Out.WriteLine(onGround);

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

			//stop any rotation
			PhysicsBody.Body.AngularVelocity = 0;

			//reset the controller jump boolean
			jump = false;

		}

		private Vector2 getGroundVector()
		{
			return new Vector2(-groundNormal.Y, groundNormal.X);
		}

		private bool isGround(WorldManifold manifold)
		{
			if (manifold.Normal.Y > 0.5)
				return false;
			return manifold.Points[0].Y < (PhysicsBody.Body.Position.Y + PhysicsBody.Shape.Radius) ||
					manifold.Points[1].Y < (PhysicsBody.Body.Position.Y + PhysicsBody.Shape.Radius);
		}

		private bool onCollision(Fixture f1, Fixture f2, Contact contact)
		{
			Fixture obstacle = (f1  == PhysicsBody) ? f2 : f1;

			//if beneath the feet, treat it as ground
			WorldManifold manifold;
			contact.GetWorldManifold(out manifold);
			if (contact.IsTouching())
			{
				if (isGround(manifold))
				{
					ground.Add(obstacle);
					groundNormal = manifold.Normal;
					groundVector = getGroundVector();
					groundSlope = groundVector.Y / groundVector.X;
					Console.Out.WriteLine("OnCollision called");
				}
			}
			return true;
		}


		private void onSeperation(Fixture f1, Fixture f2)
		{
			Fixture obstacle = (f1 == PhysicsBody) ? f2 : f1;

			ground.Remove(obstacle);
			Console.Out.WriteLine("OnSeperation called");
		}

		private void updateRotation()
		{
			if (onGround)
			{
				double angleOfRotation = Math.Acos(Vector2.Dot(Vector2.UnitY, groundNormal)) - Math.PI;
				Rotation = ((float)angleOfRotation);
			}
			else Rotation = 0;
		}

		public override void draw(GameTime gameTime)
		{
			Vector2 origin = new Vector2(animationManager.CurrentFrame.Width / 2, animationManager.CurrentFrame.Height / 2);
			//this just means mirror the sprite if we're not FacingLeft.
			SpriteEffects direction = (FacingLeft) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			environment.Camera.record(Sprite, Position, Color.White, animationManager.CurrentFrame, Rotation, origin, new Vector2(Scale, Scale),
				direction, 0);
		}
		
    }
}
