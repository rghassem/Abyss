using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Abyss.Code.Screen;

namespace Abyss.Code.Game
{
	public class LightSource : GameObject
	{

		public LightSource(GameScreen screen, Vector2 position)
			: base(screen)
		{
			//position = new Vector2(640, 310);
			Position = position;
			screen.registerLightSource(this);
		}

		public override void Update(GameTime gameTime)
		{
			
		}
	}
}
