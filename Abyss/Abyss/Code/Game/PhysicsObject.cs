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

        public Fixture PhysicsBody {get; protected set;}

		protected float height;
		protected float width;

        public override Vector2 Position 
        {
            get 
            {
                if (PhysicsBody != null)
                    return PhysicsBody.Body.Position;
                else return base.Position; 
            } 
            set 
            {
                if (PhysicsBody != null )
                    PhysicsBody.Body.Position = value;
                else base.Position = value;
            }
        }

		public PhysicsObject(GameScreen screen, float w, float h)
            : base(screen)
        {
			width = w;
			height = h;
			createBody(ref screen.world);
			PhysicsBody.UserData = this;
        }

		protected virtual void createBody(ref World world)
        {
            float BodyHeight = (Sprite != null) ? UnitConverter.ToSimUnits(height / 2) : 1;
            PhysicsBody = FixtureFactory.CreateCircle(world, BodyHeight, 1);
            PhysicsBody.Body.Position = base.Position;
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

		#region Physics Helper Methods
		/// <summary>
		/// Returns true if there is an unobstructed line from rayStart to target.
		/// </summary>
		/// <param name="target">Point that we are tracing to</param>
		/// <param name="rayStart">Point the trace starts from</param>
		/// <param name="NearestObject">Returns obstructing object if there is one,
		/// null otherwise.</param>
		/// <returns></returns>
		protected bool testLineOfSight(Vector2 target, Vector2 rayStart, out Fixture NearestObject )
		{
			Fixture nearestObjectInLine = null; //so variable can't be unassigned
			float distanceToNearest = float.MaxValue;
			environment.world.RayCast(
				(Fixture hit, Vector2 point, Vector2 hitnorm, float frac) =>
				{
					float dist = (Position - point).Length();
					if (dist < distanceToNearest)
					{
						nearestObjectInLine = hit;
						distanceToNearest = dist;
					}
					return 1;
				}, rayStart, target);
			NearestObject = nearestObjectInLine;
			return !(nearestObjectInLine != null);
		}

		/// <summary>
		/// Returns true if there is an unobstructed line to the given point from this object's
		/// center.
		/// </summary>
		protected bool testLineOfSight(Vector2 target)
		{
			Fixture dummy;
			return testLineOfSight(target, Position, out dummy);
		}

		/// <summary>
		/// Returns true if there is an unobstructed line to the given point.
		/// If not, nearestObstacle stores the fixture in the way.
		/// </summary>
		protected bool testLineOfSight(Vector2 target, out Fixture nearestObstacle)
		{
			return testLineOfSight(target, Position, out nearestObstacle);
		}

		/// <summary>		
		/// Returns true if there is an unobstructed staight line between rayStartPosition
		/// and the given object's center.
		/// </summary>
		protected bool testLineOfSight(PhysicsObject targetObject, Vector2 rayStartPosition)
		{
			Fixture nearestObjectInLine;
			testLineOfSight(targetObject.Position, rayStartPosition, out nearestObjectInLine);
			return !(nearestObjectInLine != targetObject.PhysicsBody);
		}

		protected bool testLineOfSight(PhysicsObject targetObject)
		{
			Fixture nearestObjectInLine;
			testLineOfSight(targetObject.Position, Position, out nearestObjectInLine);
			return !(nearestObjectInLine != targetObject.PhysicsBody);
		}

		#endregion
	}
}
