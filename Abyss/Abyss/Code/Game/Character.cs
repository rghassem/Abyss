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
		const float MAX_SPEED = 8;
		const float MAX_AIR_SPEED = 12;
		const float JUMP_HEIGHT = 30;
		const float LONG_JUMP_BONUS = 15;
		float movementAccel = 10;
		float airAccel = 2;

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
		bool onGround
		{
			get
			{
				if (jumping)
					return false;
				return ground.Count > 0;		
			}
			set { } //can't be set, just is true if we currently have a ground.
		}

        public Character(GameScreen screen, Vector2 pos, Texture2D sprt, ref World world)
			: base(screen, pos, sprt, ref world)
        {
            // TODO: Construct any child components here
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

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
			updateMovement(gameTime);
            base.Update(gameTime);
        }

//Movement code:

		public void updateMovement(GameTime gameTime)
		{
			//TODO: Convert normal vector into a rotatioin in degrees
			Vector2 impulse = Vector2.Zero;
			//groundSlope = 0;
			if (onGround)
			{
				groundVector = getGroundVector();
				groundSlope = groundVector.Y / groundVector.X;
			}

			wallSlopeUnderLimit = Math.Abs(groundSlope) < slopeLimit;

			if (moveRight && wallSlopeUnderLimit)
			{
				if (onGround)
					impulse += groundVector * movementAccel;
				else
					impulse += Vector2.UnitX * airAccel;
			}

			else if (moveLeft && wallSlopeUnderLimit)
			{
				if (onGround)
					impulse += groundVector * -movementAccel;
				else
					impulse += Vector2.UnitX * -airAccel;
			}

			if (!moveLeft && !moveRight && onGround && wallSlopeUnderLimit)
				PhysicsBody.Body.LinearVelocity = new Vector2(0, 0);
			
			if (jump && onGround)
			{
				jumping = true;
				longJump = (moveLeft || moveRight) ? true : false;
				timeSinceJump = 0.8f;
			}

			if (jumping)
			{
				float jumpHeight = JUMP_HEIGHT;
				if (longJump)
					jumpHeight += LONG_JUMP_BONUS;

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
				if (Math.Abs(PhysicsBody.Body.LinearVelocity.Length()) > MAX_SPEED)
				{
					Vector2 adjustedVelocity = Vector2.Normalize(PhysicsBody.Body.LinearVelocity)
						* MAX_SPEED;
					PhysicsBody.Body.LinearVelocity = adjustedVelocity;
				}
			}
			else //if in the air, apply the MAX_AIR_SPEED instead.
			{
				if (Math.Abs(PhysicsBody.Body.LinearVelocity.Length()) > MAX_AIR_SPEED)
				{
					Vector2 adjustedVelocity = Vector2.Normalize(PhysicsBody.Body.LinearVelocity)
						* MAX_AIR_SPEED;
					PhysicsBody.Body.LinearVelocity = adjustedVelocity;
				}
			}

			//stop any rotation
			PhysicsBody.Body.AngularVelocity = 0;

			//reset the controller booleans
			moveLeft = false;
			moveRight = false;
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
			double angleOfRotation = Math.Acos(Vector2.Dot(Vector2.UnitY, groundNormal));
			Rotation = ((float)angleOfRotation);
		}
		
    }
}
