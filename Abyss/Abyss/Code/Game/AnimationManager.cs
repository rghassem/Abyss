using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Abyss.Code.Game
{	
	/*Animation:
	 * This AnimationManager controls all the animation for a single object.
	 * Each GameObject has one. Animations themselves are just Rectangles which
	 * refer to a position in a sprite sheet. The sprite sheet itself is not
	 * contained here, just a Dictionary of Rectangles, each with their own name
	 * (eg Idle1, Jog2, etc). This means all animations have to be contained in the
	 * same sprite sheet (for now). So basically the AnimationManager's job is to 
	 * maintain a position in a sprite sheet over time, given whatever the current
	 * animation is. That position is then used when the GameObject calls its draw
	 * method. The position Rectangle is the CurrentFrame property.
	 * */
	public class AnimationManager
	{
		public Rectangle CurrentFrame;

		Dictionary<string, Animation> animSet;
		Dictionary<string, Rectangle> animSource;
		private Animation currentAnim;

		/// <summary>
		/// Create an AnimationManager, animations are later added using the
		/// addAnimation method.
		/// </summary>
		/// <param name="animListingXML">XML document associating frames in a spritesheet with names.</param>
		public AnimationManager(string animListingXML)
		{
			animSource = AbyssGame.Assets.Load<Dictionary<string, Rectangle>>(animListingXML+"Ref");
			animSet = new Dictionary<string, Animation>();
			//Set the initial current frame to the first image in the sprite sheet.
			int h = animSource.First().Value.Height;
			int w = animSource.First().Value.Width;
			CurrentFrame = new Rectangle(0, 0, w, h);
		}

		/// <summary>
		/// A constructor for animation managers in game objects with no animations.
		/// CurrentFrame will always be rectangle with origin at 0,0, and the given
		/// width and height.
		/// </summary>
		public AnimationManager(int width, int height)
		{
			animSource = null;
			animSet = null;
			CurrentFrame = new Rectangle(0, 0, width, height);
		}

		/// <summary>
		/// Add an animation sequence to this AnimationManager.
		/// </summary>
		/// <param name="name">The name of the animation</param>
		/// <param name="animationKeys">A list of names of frames (in order) from the 
		/// source spritesheet that define the animation</param>
		public void addAnimation(string name, float cycleTime, params string[] animationKeys)
		{
			animSet.Add(name, new Animation(name, new List<string>(animationKeys), cycleTime));
		}

		/// <summary>
		/// Update Animations. This should be called from the containing GameObject's
		/// Update method.
		/// </summary>
		public void animate(GameTime gameTime)
		{
			if (currentAnim != null)
			{
				currentAnim.update(gameTime);
				animSource.TryGetValue(currentAnim.CurrentFrame, out CurrentFrame);
			}
		}

		/// <summary>
		/// Plays an animation once. To loop, use loopAnim.
		/// </summary>
		/// <param name="animationName">The animation to play</param>
		/// <param name="cycleTime">The amount of time it should take to complete
		/// one cycle through the animation.</param>
		/// <returns>Will return false if no such animation exists.</returns>
		public bool playAnim(string animationName, float cycleTime = -1)
		{
			return runAnim(animationName, false, cycleTime);
		}

		/// <summary>
		/// Loops the animation until stopAnim is called or another animation is played.
		/// </summary>
		/// <param name="animationName">The animation to play</param>
		/// <param name="cycleTime">The amount of time it should take to complete
		/// one cycle through the animation.</param>
		/// <returns>Will return false if no such animation exists.</returns>
		public bool loopAnim(string animationName, float cycleTime = -1)
		{
			return runAnim(animationName, true, cycleTime);
		}

		/// <summary>
		/// Pause the current animation.
		/// </summary>
		public void pauseAnim()
		{
			if (currentAnim != null)
				currentAnim.pauseAnim();
		}

		/// <summary>
		/// restart the current animation.
		/// </summary>
		public void restartAnim()
		{
			if (currentAnim != null)
				currentAnim.resetAnim();
		}

		private bool runAnim(string animationName, bool loop, float cycleTime = -1)
		{
			if (currentAnim != null)
			{
				if (currentAnim.Name == animationName && currentAnim.isPlaying())
					return true;

				currentAnim.stopAnim();
			}
			if (animSet.TryGetValue(animationName, out currentAnim))
			{
				if (cycleTime != -1)
					currentAnim.AnimLength = cycleTime;
				if (loop)
					currentAnim.loopAnim();
				else currentAnim.playAnim();
				return true;
			}
			else return false;
		}

	}
}
