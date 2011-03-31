using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled = Squared.Tiled;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Abyss.Code.Game;
using Abyss.Code.Screen;

namespace Abyss {
	class MapLayer : GameObject {
		public GameScreen Screen;
		private Tiled.Map m_map;
		private string m_layer;

		public MapLayer(GameScreen screen, Tiled.Map map, string layer, float zindex)
				: base(screen) {
			Screen = screen;
			m_map = map;
			m_layer = layer;
			Zindex = zindex;
		}

		public override void draw(GameTime gameTime) {
			Rectangle rectangle = Screen.Camera.Screen;

			// Draw map.
			m_map.Layers[m_layer].Draw(Screen.Camera.spriteBatch, m_map.Tilesets.Values, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), m_map.TileWidth, m_map.TileHeight, UnitConverter.ToDisplayUnits(Position), Zindex);
		}
	}
}
