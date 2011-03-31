using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Abyss.Code.Screen;

namespace Abyss {
	public class SpawnController {
		internal GameScreen GameScreen;
		public List<SpawnPoint> SpawnPoints = new List<SpawnPoint>();

		public const int k_spawnRadius = 300;

		public SpawnController(GameScreen screen) {
			GameScreen = screen;
		}
		
		public void CreateSpawnPoints(IList<Squared.Tiled.ObjectGroup> objectGroupList, Vector2? offset = null, bool createPlayerSpawn = false) {
			bool spawnedPlayer = false;
			Vector2 realOffset = offset == null ? Vector2.Zero : (Vector2) offset;
			
			// Load spawn points.
			foreach (Squared.Tiled.ObjectGroup objGroup in objectGroupList) {
				foreach (List<Squared.Tiled.Object> objList in objGroup.Objects.Values) {
					foreach (Squared.Tiled.Object obj in objList) {
						obj.X += (int) realOffset.X;
						obj.Y += (int) realOffset.Y;
						SpawnPoint sp = new SpawnPoint(this, obj);

						// Immediately spawn some entities.
						switch (sp.GameObjectType) {
							case "spawn":
								if (createPlayerSpawn) {
									spawnedPlayer = true;
									sp.AlwaysSpawned = true;
									sp.Spawn();
								}
								break;
							default:
								SpawnPoints.Add(sp);
								break;
						}
					}
				}
			}

			if (!spawnedPlayer && createPlayerSpawn) throw new InvalidOperationException("Level loaded does not contain player spawn point.");
		}

		public void Update(float elapsedTime) {
			int halfwidth = (int) (AbyssGame.ScreenWidth / 2 + k_spawnRadius);
			int halfheight = (int) (AbyssGame.ScreenHeight / 2 + k_spawnRadius);

			int x = (int) GameScreen.Camera.Screen.Center.X;
			int y = (int) GameScreen.Camera.Screen.Center.Y;

			Rectangle spawnRect = new Rectangle(x - halfwidth, y - halfheight, 2 * halfwidth, 2 * halfheight);

			SpawnPoints.ForEach((SpawnPoint sp) => sp.Update(elapsedTime, spawnRect));
		}
	}
}
