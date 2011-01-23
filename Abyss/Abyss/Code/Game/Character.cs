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
using FarseerPhysics.Dynamics.Joints;
using Abyss.Code.Screen;

namespace Abyss.Code.Game
{
    /// <summary>
    /// A Character in the Game. Extends PhysicsObjects because characters also have physics.
    /// </summary>
    public class Character : PhysicsObject
    {
        public Character(GameScreen screen, Vector2 pos, Texture2D sprt, ref World world)
			: base(screen, pos, sprt, ref world)
        {
            // TODO: Construct any child components here
            PhysicsBody.Body.BodyType = BodyType.Dynamic;
        }

    /*    protected override void createBody(ref World world)
        {
            float BodyHeight = (Sprite != null) ? UnitConverter.ToSimUnits(Sprite.Height) : 1;
            float BodyWidth = (Sprite != null) ? UnitConverter.ToSimUnits(Sprite.Width) : 1;
            PhysicsBody = FixtureFactory.CreateCircle(world, BodyHeight, 1);
            PhysicsBody.Body.Position = position;

            //The following code is adapted from: http://www.sgtconker.com/2010/09/article-xna-farseer-platform-physics-tutorial/

            //Create a body that is almost the size of the entire object
            //but with the bottom part cut off.
            float upperBodyHeight = BodyHeight - (BodyWidth / 2);
            //// body = BodyFactory.Instance.CreateRectangleBody(physics, BodyWidth, upperBodyHeight, mass / 2);
            PhysicsBody = FixtureFactory.CreateRectangle(world, BodyWidth, upperBodyHeight, 1); //1 is density
            //also shift it up a tiny bit to keep the new object's center correct
            //// body.Position = position - Vector2.UnitY * (BodyWidth / 4);
            PhysicsBody.Body.Position = position - Vector2.UnitY * (BodyWidth / 4);
          ///  float centerOffset = position.Y - PhysicsBody.Body.Position.Y; //remember the offset from the center for drawing

            //Now let's make sure our upperbody is always facing up.
            //// fixedAngleJoint = JointFactory.Instance.CreateFixedAngleJoint(physics, body);
           // FixedAngleJoint waistJoint = JointFactory.CreateFixedAngleJoint(world, PhysicsBody.Body);
            //Create a wheel as wide as the whole object
            Fixture wheelBody = FixtureFactory.CreateCircle(world, BodyWidth / 2, 1);

           //And position its center at the bottom of the upper body
            wheelBody.Body.Position = PhysicsBody.Body.Position + Vector2.UnitY * (upperBodyHeight / 2);

            //These two bodies together are width wide and height high. So let's connect them together.
            RevoluteJoint motor = JointFactory.CreateRevoluteJoint(world, PhysicsBody.Body, wheelBody.Body, wheelBody.Body.Position);
            motor.MotorEnabled = true;
            motor.MaxMotorTorque = 100f; //set this higher for some more juice
            motor.MotorSpeed = 0;

            //Create geomitries. Not using this.
            wheelGeom = GeomFactory.Instance.CreateCircleGeom(physics, wheelBody, BodyWidth / 2, 16);
            geom = GeomFactory.Instance.CreateRectangleGeom(physics, body, width, upperBodyHeight);
            wheelGeom.IgnoreCollisionWith(geom);
            geom.IgnoreCollisionWith(wheelGeom);
 
            //Set the friction of the wheelGeom to float.MaxValue for fast stopping/starting
            //or set it higher to make the character slip.
            wheelBody.Friction = float.MaxValue;

            PhysicsBody.Body.BodyType = BodyType.Dynamic;
            wheelBody.Body.BodyType = BodyType.Dynamic;
        }*/

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
