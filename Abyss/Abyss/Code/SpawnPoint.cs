using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;
using Abyss.Code.Game;
using Abyss.Code.Screen;

namespace Abyss {
	public class SpawnPoint {
		#region Properties

		private SpawnController SpawnController;

		// Position.
		public Vector2 Position;
		private Vector2 OriginalPosition;

		// Size.
		public Vector2 Size;

		public Vector2 TopLeft {
			get {
				return Position - Size / 2;
			}
		}

		public Vector2 BottomLeft {
			get {
				return Position + new Vector2(-Size.X, Size.Y) / 2;
			}
		}

		public Vector2 TopRight {
			get {
				return Position + new Vector2(Size.X, -Size.Y) / 2;
			}
		}

		public Vector2 BottomRight {
			get {
				return Position + Size / 2;
			}
		}

		public Rectangle Rect {
			get {
				return new Rectangle((int) TopLeft.X, (int) TopLeft.Y, (int) Size.X, (int) Size.Y);
			}
		}

		// Name.
		public string Name;

		// Type, used for spawning.
		public string GameObjectType;

		// Entity once spawned.
		public GameObject GameObject;

		// Should the entity respawn again after being culled?
		public bool AllowRespawn = true;

		// Time in seconds until SpawnPoint can trigger again.
		public float RespawnCooldown = 0.0f;

		// Arbitrary data added by level editor to initialize object.
		public SortedList<string, string> Properties = new SortedList<string, string>();


		// Current progress on respawn time.
		private float m_currentCooldown;

		// Has the entity been offscreen since it was culled?
		public bool HasBeenOffscreen = true;

		// Should always be spawned?  Used for player ship so that schlee respawns when dying.
		public bool AlwaysSpawned = false;

		#endregion

		public SpawnPoint(SpawnController spawner, string type, Vector2 position) {
			SpawnController = spawner;
			GameObjectType = type;
			Position = position;
			OriginalPosition = position;
		}

		internal SpawnPoint(SpawnController spawner, Squared.Tiled.Object obj) {
			SpawnController = spawner;
			Size = new Vector2(obj.Width, obj.Height);
			Position = new Vector2(obj.X, obj.Y) + Size / 2;
			OriginalPosition = new Vector2(obj.X, obj.Y) + Size / 2;

			Name = obj.Name;
			Properties = obj.Properties;
			GameObjectType = obj.Type;
		}

		internal void Update(float elapsedTime, Rectangle spawnRect) {
			m_currentCooldown += elapsedTime;
			if (!HasBeenOffscreen) {
				HasBeenOffscreen = !spawnRect.Intersects(Rect);
			}
			
			if (AlwaysSpawned || (HasBeenOffscreen && m_currentCooldown >= RespawnCooldown && spawnRect.Intersects(Rect))) {
				Spawn();
			}
		}

		internal GameObject Spawn() {
			SpawnController.SpawnPoints.Remove(this);

			// Update SpawnPoint in case it gets triggered again.
			HasBeenOffscreen = false;

			switch (GameObjectType) {
				case "spawn":
					GameObject = new PlayerCharacter(SpawnController.GameScreen, this);
					break;
				case "goblin":
					GameObject = new Goblin(SpawnController.GameScreen, this);
					break;
				case "light":
					GameObject = new LightSource(SpawnController.GameScreen, this.Position);
					break;
				default:
					throw new InvalidOperationException("Invalid entity type.");
			}

			SpawnController.GameScreen.addObject(GameObject);

			return GameObject;
		}

		public void Reset() {
			if (AllowRespawn) {
				m_currentCooldown = 0.0f;
				SpawnController.SpawnPoints.Add(this);
			}

			GameObject = null;
		}
	}
}
