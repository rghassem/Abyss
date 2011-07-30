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
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;


namespace Abyss.Code.Game
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class ParticleEntity : GameObject
	{
		public ParticleEffect Effect; 
		private AbyssGame abyss;
		private Screen.GameScreen gameScreen; 

		public ParticleEntity(Screen.GameScreen screen, string effectName)
			: base(screen)
		{
			abyss = screen.Game;
			gameScreen = screen;
			Effect = abyss.Content.Load<ParticleEffect>(effectName).DeepCopy();
			Effect.Initialise();
			Effect.LoadContent(abyss.Content);
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			Effect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			base.Update(gameTime);
		}

		public override void draw(GameTime gameTime)
		{
			gameScreen.Camera.endRecord();
			Matrix tform = gameScreen.Camera.View;
			abyss.renderer.RenderEffect(Effect, ref tform);
			gameScreen.Camera.beginRecord();
			base.draw(gameTime);
		}
	}
}
