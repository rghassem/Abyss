using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Abyss.Code.Game;
using Abyss.Code.UserInterface.OSD;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using Tiled = Squared.Tiled;
using System.IO;

namespace Abyss.Code.Screen 
{

    /// <summary>
    /// The Screen where all the gameplay happens. Contains a lot of GameObjects, and has a FarseerCamera which is used to
    /// display everything except the HUD (including the FaseerPhysics DebugView).
    /// </summary>
    public class GameScreen : Screen
    {
		public AbyssGame Game;
        private List<GameObject> GameObjects;
        public World world;
        public FarseerCamera Camera;
        public PlayerCharacter PC;
		private SpawnController SpawnController;
		private Body WorldBody;

        private OSD osd;

        public const int PIXELS_PER_METER = 50;
		public const int GRAVITY = 30;

		public ParticleEntity explosionParticleEffect;

        public GameScreen(AbyssGame game)
        {
			Game = game;
            world = new World(new Vector2(0, GRAVITY));
            UnitConverter.SetDisplayUnitToSimUnitRatio(PIXELS_PER_METER);
			Camera = new FarseerCamera(ref AbyssGame.spriteBatch, AbyssGame.Assets, Game.GraphicsDevice,
				AbyssGame.ScreenWidth, AbyssGame.ScreenHeight, ref world, PIXELS_PER_METER);
            Camera.viewMode = FarseerCamera.ViewMode.Scroll;
            
            GameObjects = new List<GameObject>(5); //probably a good starting number

			SpawnController = new SpawnController(this);

			Camera.subjetDistanceToScreenEdge = 700;

			explosionParticleEffect = new ParticleEntity(this, "BasicFireball");
			addObject(explosionParticleEffect);

			osd = new OSD();
			osd.LoadContent();
        }

		public void addObject(GameObject obj) {
			Game.addComponent(obj);
			GameObjects.Add(obj);
		}

		public void removeObject(GameObject obj) {
			Game.Components.Remove(obj);
			GameObjects.Remove(obj);
		}

        /// <summary>
        /// This method will load a level from memory, and add to the GameObjects list accordingly.
        /// For now, I just manually set what I want in the level from here.
        /// </summary>
        public void loadLevel()
        {
			/*PC = new PlayerCharacter(this);
			PC.Position = new Vector2(15, 10);
			addObject(PC);
            addObject(new RigidBlock(this, new Vector2(15, 12), 50, 1, 0.0f));
			addObject(new RigidBlock(this, new Vector2(20,8), 50, 1, 100.0f));
			*/
			LoadMap("testlevel.tmx");
        }

		private Tiled.Tileset.TilePropertyList GetTileProperties(Tiled.Map map, int tileId) {
			foreach (Tiled.Tileset t in map.Tilesets.Values) {
				Tiled.Tileset.TilePropertyList props = t.GetTileProperties(tileId);
				if (props != null) return props;
			}

			return null;
		}

		/// <summary>
		/// Add a rectangle collision shape to the body.
		/// </summary>
		/// <param name="halfsize">A vector specifying half of the width and height of the rectangle in body space.</param>
		/// <param name="center">The center point of the rectangle in body space.</param>
		/// <param name="rotation">Rotation of the shape relative to the body in radians.</param>
		/// <param name="density">Density of the shape.</param>
		/// <returns>Fixture definition.</returns>
		private Fixture AddCollisionRectangle(Vector2 halfsize, Vector2 center, float rotation = 0.0f, float density = 1.0f) {
			PolygonShape poly = new PolygonShape();
			poly.SetAsBox(UnitConverter.ToSimUnits(halfsize.X), UnitConverter.ToSimUnits(halfsize.Y), UnitConverter.ToSimUnits(center), rotation);
			return WorldBody.CreateFixture(poly, density);
		}

		/// <summary>
		/// Load a map from file and create collision objects for it.
		/// </summary>
		/// <param name="filename">File to load map from.</param>
		public void LoadMap(string filename) {
			if (WorldBody != null) world.RemoveBody(WorldBody);
			LoadOrExtendMap(filename, true);
		}

		enum CollisionType : byte {
			Unknown = 0
			, None = 1
			, Solid
			, SlopeUp
			, SlopeDown
			, HalfSlopeUp1
			, HalfSlopeUp2
			, HalfSlopeDown1
			, HalfSlopeDown2
		}



		/// <summary>
		/// Load a map from file and create collision objects for it.  Appends map horizontally if one exists.
		/// </summary>
		/// <param name="filename">File to load map from.</param>
		public void LoadOrExtendMap(string filename, bool spawnPlayer = false) {
			Tiled.Map map = Tiled.Map.Load(Path.Combine(Game.Content.RootDirectory, filename), Game.Content);

			// Destroy and re-create collision body for map.
			if (WorldBody == null) {
				WorldBody = world.CreateBody();
				WorldBody.IsStatic = true;
			}

			Vector2 tileHalfSize = new Vector2(map.TileWidth, map.TileHeight) / 2;
			Vector2 tileSize = new Vector2(map.TileWidth, map.TileHeight);

			CollisionType[,] levelCollision = new CollisionType[map.Width, map.Height];

			float defaultZVal = 0.001f; //changed from 0.9f, was drawing after foreground objects

			// Tile id to collision type mapping.
			List<CollisionType> collision = new List<CollisionType>();

			foreach (KeyValuePair<string, Tiled.Layer> layer in map.Layers) {
				defaultZVal -= 0.001f;

				for (int x = 0; x < layer.Value.Width; ++x)
					for (int y = 0; y < layer.Value.Height; ++y) {
						int tileId = layer.Value.GetTile(x, y);

						if (tileId >= collision.Count || collision[tileId] == 0) {
							Tiled.Tileset.TilePropertyList props = GetTileProperties(map, tileId);

							// The only way to add new elements at arbitrary indices is to fill up the rest of the array.  Do so.
							for (int i = collision.Count; i < tileId + 1; ++i) collision.Add(0);

							if (props != null && props.ContainsKey("collision")) {
								string value = props["collision"];
								if (value.Equals("solid", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.Solid;
								} else if (value.Equals("slopeup", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.SlopeUp;
								} else if (value.Equals("slopedown", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.SlopeDown;
								} else if (value.Equals("halfslopeup1", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.HalfSlopeUp1;
								} else if (value.Equals("halfslopeup2", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.HalfSlopeUp2;
								} else if (value.Equals("halfslopedown1", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.HalfSlopeDown1;
								} else if (value.Equals("halfslopedown2", StringComparison.OrdinalIgnoreCase)) {
									collision[tileId] = CollisionType.HalfSlopeDown2;
								} else {
									collision[tileId] = CollisionType.None;
								}
							} else {
								collision[tileId] = CollisionType.None;
							}
						}

						levelCollision[x, y] = collision[tileId];
					}

				float z = defaultZVal;

				if (layer.Value.Properties.ContainsKey("zindex")) {
					if (!float.TryParse(layer.Value.Properties["zindex"], out z)) {
						z = defaultZVal;
					}
				}

				MapLayer ml = new MapLayer(this, map, layer.Key, z);
				addObject(ml);
			}

			// Go through collision and try to create large horizontal collision shapes.
			for (int y = 0; y < map.Height; ++y) {
				int firstX = 0;
				bool hasCollision = false;

				for (int x = 0; x < map.Width; ++x) {
					if (levelCollision[x, y] == CollisionType.Solid) {
						if (hasCollision) continue;
						else {
							hasCollision = true;
							firstX = x;
						}
					} else {
						if (hasCollision) {
							hasCollision = false;
							int tilesWide = x - firstX;
							if (tilesWide == 1) continue;

							for (int i = firstX; i <= x; ++i) levelCollision[i, y] = CollisionType.None;

							AddCollisionRectangle(
								tileHalfSize * new Vector2(tilesWide, 1.0f)
								, new Vector2(tileSize.X * (x - (float) tilesWide / 2), tileSize.Y * (y + 0.5f))
							);
						}
					}
				}

				// Create final collision.
				if (hasCollision) {
					for (int i = firstX; i < map.Width; ++i) levelCollision[i, y] = CollisionType.None;

					int tilesWide = map.Width - firstX;
					AddCollisionRectangle(
						tileHalfSize * new Vector2(tilesWide, 1.0f)
						, new Vector2(tileSize.X * (map.Width - (float) tilesWide / 2), tileSize.Y * (y + 0.5f))
					);
				}
			}

			// Go through collision and try to create large vertical collision shapes.
			for (int x = 0; x < map.Width; ++x) {
				int firstY = 0;
				bool hasCollision = false;

				for (int y = 0; y < map.Height; ++y) {
					if (levelCollision[x, y] == CollisionType.Solid) {
						if (hasCollision) continue;
						else {
							hasCollision = true;
							firstY = y;
						}
					} else {
						if (hasCollision) {
							hasCollision = false;
							int tilesTall = y - firstY;

							AddCollisionRectangle(
								tileHalfSize * new Vector2(1.0f, tilesTall)
								, new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y - (float) tilesTall / 2))
							);
						}
					}
				}

				// Create final collision.
				if (hasCollision) {
					int tilesTall = map.Height - firstY;
					AddCollisionRectangle(
						tileHalfSize * new Vector2(1.0f, tilesTall)
						, new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (map.Height - (float) tilesTall / 2))
					);
				}
			}

			// Traverse map and create non-solid tile shapes.
			for (int x = 0; x < map.Width; ++x) {
				for (int y = 0; y < map.Height; ++y) {
					switch (levelCollision[x, y]) {
						case CollisionType.Solid:
							// Already handled.
							break;

						case CollisionType.SlopeUp: {
							Vector2 halfsize = UnitConverter.ToSimUnits(tileHalfSize);
							Vector2 center = UnitConverter.ToSimUnits(new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y + 0.5f)));

							PolygonShape poly = new PolygonShape();
							poly.Set(new FarseerPhysics.Common.Vertices(new Vector2[] {
								center + new Vector2(-halfsize.X, halfsize.Y)
								, center + new Vector2(halfsize.X, -halfsize.Y)
								, center + new Vector2(halfsize.X, halfsize.Y)
							}));
							WorldBody.CreateFixture(poly);
							break;
						}

						case CollisionType.HalfSlopeUp1: {
							Vector2 halfsize = UnitConverter.ToSimUnits(tileHalfSize);
							Vector2 center = UnitConverter.ToSimUnits(new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y + 0.5f)));

							PolygonShape poly = new PolygonShape();
							poly.Set(new FarseerPhysics.Common.Vertices(new Vector2[] {
								center + new Vector2(-halfsize.X, halfsize.Y)
								, center + new Vector2(halfsize.X, 0.0f)
								, center + new Vector2(halfsize.X, halfsize.Y)
							}));
							Fixture f = WorldBody.CreateFixture(poly);
							break;
						}

						case CollisionType.HalfSlopeUp2: {
							Vector2 halfsize = UnitConverter.ToSimUnits(tileHalfSize);
							Vector2 center = UnitConverter.ToSimUnits(new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y + 0.5f)));

							PolygonShape poly = new PolygonShape();
							poly.Set(new FarseerPhysics.Common.Vertices(new Vector2[] {
								center + new Vector2(-halfsize.X, halfsize.Y)
								, center + new Vector2(-halfsize.X, 0.0f)
								, center + new Vector2(halfsize.X, -halfsize.Y)
								, center + new Vector2(halfsize.X, halfsize.Y)
							}));
							WorldBody.CreateFixture(poly);
							break;
						}

						case CollisionType.SlopeDown: {
							Vector2 halfsize = UnitConverter.ToSimUnits(tileHalfSize);
							Vector2 center = UnitConverter.ToSimUnits(new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y + 0.5f)));

							PolygonShape poly = new PolygonShape();
							poly.Set(new FarseerPhysics.Common.Vertices(new Vector2[] {
								center + new Vector2(-halfsize.X, halfsize.Y)
								, center + new Vector2(-halfsize.X, -halfsize.Y)
								, center + new Vector2(halfsize.X, halfsize.Y)
							}));
							WorldBody.CreateFixture(poly);
							break;
						}

						case CollisionType.HalfSlopeDown1: {
							Vector2 halfsize = UnitConverter.ToSimUnits(tileHalfSize);
							Vector2 center = UnitConverter.ToSimUnits(new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y + 0.5f)));

							PolygonShape poly = new PolygonShape();
							poly.Set(new FarseerPhysics.Common.Vertices(new Vector2[] {
								center + new Vector2(-halfsize.X, halfsize.Y)
								, center + new Vector2(-halfsize.X, -halfsize.Y)
								, center + new Vector2(halfsize.X, 0.0f)
								, center + new Vector2(halfsize.X, halfsize.Y)
							}));
							WorldBody.CreateFixture(poly);
							break;
						}


						case CollisionType.HalfSlopeDown2: {
							Vector2 halfsize = UnitConverter.ToSimUnits(tileHalfSize);
							Vector2 center = UnitConverter.ToSimUnits(new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y + 0.5f)));

							PolygonShape poly = new PolygonShape();
							poly.Set(new FarseerPhysics.Common.Vertices(new Vector2[] {
								center + new Vector2(-halfsize.X, halfsize.Y)
								, center + new Vector2(-halfsize.X, 0.0f)
								, center + new Vector2(halfsize.X, halfsize.Y)
							}));
							WorldBody.CreateFixture(poly);
							break;
						}
					}
				}
			}

			SpawnController.CreateSpawnPoints(map.ObjectGroups.Values, Vector2.Zero, spawnPlayer);
		}
      
	    public void update(GameTime gameTime)
        {
            //no need to update GameObjects, they're GameComponents, and will get the update automatically.
            world.Step(gameTime.ElapsedGameTime.Milliseconds * .001f);
            Camera.update();

			if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
				explosionParticleEffect.Effect.Trigger(new Vector2(Camera.Screen.X + Camera.Screen.Width / 2, Camera.Screen.Y + Camera.Screen.Height / 2));
			}

			SpawnController.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void draw(GameTime gameTime) {
			Camera.beginDraw();

			foreach (GameObject obj in GameObjects.OrderBy(obj => obj.Zindex)) {
				obj.draw(gameTime);
			}

			Camera.endDraw();
			Camera.drawDebug();

			AbyssGame.spriteBatch.Begin();
			osd.Draw(gameTime, PC);
			AbyssGame.spriteBatch.End();
        }
    }
}
