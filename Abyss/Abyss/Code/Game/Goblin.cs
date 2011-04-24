using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using Abyss.Code.Screen;
using FarseerPhysics.Collision;

namespace Abyss.Code.Game
{
	/// <summary>
	/// The basic enemy. Will patrol around a given point ("Post") until
	/// until it sees player. If it sees the player, it will pursue.
	/// </summary>
	public class Goblin : Character
	{
		const int COLLISION_WIDTH = 96;
		const int COLLISION_HEIGHT = 96;
		const float PATROL_SPEED = 2;
		const float ATTACK_SPEED = 4;
		//if the player is within this radius, he is detected instantly
		const float IMMEDIATE_DETECTION_RADIUS = 3;
		//if the player is in front of the goblin, and within this distance, he may be seen
		//assuming there is nothing in the way
		const float FORWARD_DETECTION_DISTANCE = 8;
		//if the angle between the line of sight between the player and the goblin, and a
		//straight line of sight, is greater than this, the player cannot be seen.
		const double VIEW_ANGLE_FOR_DETECTION = Math.PI/5; //30 degrees
		const float ATTACK_RANGE = 2.0f;
		const uint DEFAULT_ATTACK_DAMAGE = 10;
		const float DEFAULT_ATTACK_FORCE = 50;

		protected enum EnemyState
		{
			PATROLING,
			CHARGING,
			ATTACKING,
			RETURNING_TO_POST,
			WAITING
		}

		EnemyState state;

		/// <summary>
		/// The center and start of the goblin's patrol route. By default the spawn point.
		/// </summary>
		Vector2 Post;
		/// <summary>
		/// Goblin will patrol between to Post.X+PatrolRadius and Post.X-PatrolRadius.
		/// </summary>
		float PatrolRadius;
		/// <summary>
		/// No matter what, the goblin cannot move farther away from his post than this number.
		/// </summary>
		float MaxRange;
		/// <summary>
		/// Amount of time to wait when reaching the edge of a patrol, before turning around.
		/// </summary>
		float waitTimeBetweenPatrols = 1.0f;

		/// <summary>
		/// Point the goblin is trying to reach. Usually only the X value is considered.
		/// </summary>
		protected Vector2 destination;

		protected uint attackDamage = DEFAULT_ATTACK_DAMAGE;
		protected float attackForce = DEFAULT_ATTACK_FORCE;

#region StateVariables
		private float timeWaited = 0;
		private float currentWaitTime = 1.0f;
		private bool waitingInPatrol = false;
		private bool facingLeftBeforeWait;
#endregion

		public Goblin(GameScreen screen, SpawnPoint spawn)
			: base(screen, COLLISION_WIDTH, COLLISION_HEIGHT)
        {
			MaxSpeed = PATROL_SPEED;
			MaxAirSpeed = 9;
			JumpHeight = 50;
			LongJumpBonus = 15;
			MovementAccel = 10;
			AirAccel = 2;

			//set scale
			Scale = 1.0f;

			Zindex = 0.4f;

			loadSprite("Animations/goblin_placeholder");

			Position = UnitConverter.ToSimUnits(spawn.Position);
			Post = Position;

			if (spawn.Properties.ContainsKey("PatrolRadius"))
				PatrolRadius = float.Parse(spawn.Properties["PatrolRadius"]);

			if (spawn.Properties.ContainsKey("MaxRange"))
				MaxRange = float.Parse(spawn.Properties["MaxRange"]);

			if (spawn.Properties.ContainsKey("waitTimeBetweenPatrols"))
				MaxRange = float.Parse(spawn.Properties["waitTimeBetweenPatrols"]);

			if (spawn.Properties.ContainsKey("PostX") && spawn.Properties.ContainsKey("PostY"))
			{
				Post = new Vector2(float.Parse(spawn.Properties["PostX"]), 
					float.Parse(spawn.Properties["PostY"]));
			}

			state = EnemyState.PATROLING;
        }

		public override void Update(GameTime gameTime)
		{
			switch (state)
			{
				case EnemyState.PATROLING:
					patrol();
					break;
				case EnemyState.CHARGING:
					charge();
					break;
				case EnemyState.ATTACKING:
					attack();
					break;
				case EnemyState.RETURNING_TO_POST:
					returnToPost();
					break;
				case EnemyState.WAITING:
					timeWaited += (float)(gameTime.ElapsedGameTime.Milliseconds * 0.001);
					wait(currentWaitTime);
					break;
			}
			//if(destination != null)
				//environment.explosionParticleEffect.Effect.Trigger(UnitConverter.ToDisplayUnits(destination));

			base.Update(gameTime);
		}

		protected override bool onCollision(Fixture f1, Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			//detect when we hit walls
			return base.onCollision(f1, f2, contact);
		}

		protected bool isFacing(Vector2 ObjectPosition)
		{
			if (FacingLeft)
				return (Position.X > ObjectPosition.X);
			else return (Position.X < ObjectPosition.X);
		}

		/// <summary>
		/// Go toward Destination
		/// </summary>
		protected void move()
		{
			if (Position.X < destination.X)
				stepRight();
			else stepLeft();
		}

		/// <summary>
		/// Return true if the goblin is stuck if he
		/// continues in his current direction.
		/// </summary>
		/// <returns></returns>
		protected bool isObstructed()
		{
			if (destination == null)
				throw new Exception("AI is checking for obstruction with no destination");
			float obstructionTestDistance = (float)(PhysicsBody.Shape.Radius * 1.3);
			Vector2 distToDest = (destination - Position);
			obstructionTestDistance = Math.Min(obstructionTestDistance, distToDest.Length());
			Vector2 moveDirection = (FacingLeft) ? -groundVector : groundVector;
			if (moveDirection == Vector2.Zero)
				return false;
			Vector2 testPoint = Position + (moveDirection * obstructionTestDistance);
			Fixture obstacle;
			bool result = testLineOfSight(testPoint, out obstacle);
			if (!result)
			{
				return (obstacle.UserData == null);
			}
			return false;
		}

		private void patrol()
		{
			if (state != EnemyState.PATROLING)
			{
				state = EnemyState.PATROLING;
				MaxSpeed = PATROL_SPEED;
				DrawColor = Color.White;
			}

			//see if we can detect the player
			if (isPlayerDetected())
			{//found him: charge, and don't continue patrolling.
				charge();
				return;
			}

			//Standard Patrol movement
			if (waitingInPatrol) //we just got done with a routine patrol wait.
			{
				FacingLeft = !facingLeftBeforeWait;
				if (FacingLeft)
					stepLeft();
				else 
					stepRight();
				waitingInPatrol = false;
			}

			if (FacingLeft)
			{
				destination = Post - new Vector2(PatrolRadius,0);
				if (Position.X < destination.X)
				{
					waitingInPatrol = true;
					facingLeftBeforeWait = true;
					wait(waitTimeBetweenPatrols);
					return;
				}
			}
			else
			{
				destination = Post + new Vector2(PatrolRadius, 0);
				if (Position.X > destination.X)
				{
					waitingInPatrol = true;
					facingLeftBeforeWait = false;
					wait(waitTimeBetweenPatrols);
					return;
				}
			}

			//encountered obstacle
			if (isObstructed())
			{
				if (!isFacing(Post)) //just turn around and go home.
				{
					destination = (FacingLeft) ? Post - new Vector2(PatrolRadius, 0) :
						Post + new Vector2(PatrolRadius, 0);
					waitingInPatrol = true;
					facingLeftBeforeWait = FacingLeft;
					wait(waitTimeBetweenPatrols);
					return;
				}
				else //can't get back to post
				{//pass it off to the return to post state, take care of issue there
					returnToPost(); 
					return;
				}
			}
			move();
		}
		private void charge()
		{
			if (state != EnemyState.CHARGING)
			{
				state = EnemyState.CHARGING;
				MaxSpeed = ATTACK_SPEED;
				DrawColor = Color.Red;
			}

			if (!isPlayerDetected())
			{
				returnToPost();
				return;
			}

			Vector2 playerPosition = environment.PC.Position;

			//if the player is outside the max range, do not pursue. 0 means no max range.
			if ((playerPosition - Post).Length() > MaxRange && MaxRange != 0)
			{
				returnToPost();
				return;
			}

			//if (isObstructed()) 
			//For now just keeps trying to attack if obstructed but can
			//still see player. This is a rare case.

			//is the player actually in range?
			float distanceToPlayer = (playerPosition - Position).Length();
			if (distanceToPlayer < ATTACK_RANGE)
			{
				if (!isFacing(playerPosition))
					changeDirection();
				attack();
				return;
			}

			destination = playerPosition;
			move();
		}
		private void attack()
		{
			//carry out the attack
			Vector2 impulse = (FacingLeft) ? new Vector2( -attackForce, 0) :
				new Vector2( attackForce, 0);
			environment.PC.takeHit(this, attackDamage, impulse);
			environment.explosionParticleEffect.Effect.Trigger(
				UnitConverter.ToDisplayUnits(environment.PC.Position));
			//stagger for half a second
			wait(1f);
		}

		private void returnToPost()
		{
			if (state != EnemyState.RETURNING_TO_POST)
			{
				state = EnemyState.RETURNING_TO_POST;
				MaxSpeed = PATROL_SPEED;
				DrawColor = Color.White;
				destination = Post;
			}

			//impossible to return to post
			if (isObstructed())
			{
				Post = Position;
				destination = Post;
			}

			//see if we can detect the player
			if (isPlayerDetected())
			{//found him: charge, and don't continue patrolling.
				charge();
				return;
			}

			float varianceAroundPost = 2.0f;
			if (Position.X < Post.X + varianceAroundPost && Position.X
				> Post.X - varianceAroundPost)
			{
				patrol();
				return;
			}
			move();
		}

		private void wait(float waitTime)
		{
			if (state != EnemyState.WAITING)
			{
				state = EnemyState.WAITING;
				timeWaited = 0;
				currentWaitTime = waitTime;
			}
			if (timeWaited > waitTime)
			{
				if (isPlayerDetected())
					charge();
				else
				{
					patrol();
					return;
				}
			}
		}

		protected bool isPlayerDetected()
		{
			Vector2 playerPosition = environment.PC.Position;
			float distanceToPlayer = (playerPosition - Position).Length();

			if ((playerPosition - Post).Length() > MaxRange)
				return false; //cannot see the player if he is out of range.

			if (distanceToPlayer < IMMEDIATE_DETECTION_RADIUS)
				return true;

			if (isFacing(playerPosition))
			{
				if (distanceToPlayer > FORWARD_DETECTION_DISTANCE)
					return false; //player too far away

				//player is in front of us and within sight range. Check that we
				//can actually see him.
				Vector2 targetVector = playerPosition - Position;
				targetVector.Normalize();
				double viewAngle = Vector2.Dot(targetVector, groundNormal);
				float viewAngleToPlayer = (float) (Math.Acos(viewAngle) - (Math.PI / 2));
				if (Math.Abs(viewAngleToPlayer) > VIEW_ANGLE_FOR_DETECTION) 
					return false; //player is too far above or below

				//try the straight line test as final criteria for seeing the player
				return testSeeCharacter(environment.PC);
			}

			return false;
		}
	}
}
