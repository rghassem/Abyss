using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Abyss.Code.Game
{
	class Animation
	{
		/// <summary>
		/// The current frame here is actually just a string, which is
		/// used to index into the animSource in AnimationManager, which
		/// in turn indexes into the spritesheet.
		/// </summary>
		public string CurrentFrame {get; private set;}
		public string Name { get; private set; }

		/// <summary>
		/// The amount of time in seconds to finish one cycle through the animation.
		/// </summary>
		public float AnimLength{
			get { return animLength; }
			set
			{
				animLength = value;
				delay = value / frames.Count;
			}
		}
		private float animLength;

		private int frameIndex;
		private List<string> frames;
		private float defaultAnimLength;
		private float delay;
		private float timeSinceLastTransitioin;
		private bool play;
		private bool loop;

		public Animation(string name, List<string> animFrameKeys, float cycleTime)
		{
			frames = animFrameKeys;
			AnimLength = cycleTime;
			defaultAnimLength = cycleTime;
			CurrentFrame = frames.First();
			frameIndex = 0;
			timeSinceLastTransitioin = 0;
			Name = name;
		}

		public void update(GameTime gameTime)
		{
			if (play)
			{
				timeSinceLastTransitioin += (float)(gameTime.ElapsedGameTime.Milliseconds*0.001);
				if (timeSinceLastTransitioin >= delay)
				{
					timeSinceLastTransitioin = 0;
					play = transition();
				}
			}
		}

		/// <summary>
		/// Play the animation for one cycle
		/// </summary>
		public void playAnim()
		{
			play = true;
		}

		/// <summary>
		/// Begin looping the animation until stopAnim() or pauseAnim() is called.
		/// </summary>
		public void loopAnim()
		{
			loop = true;
			playAnim();
		}

		/// <summary>
		/// Stops this animation and sets the current frame to the first frame.
		/// Also resets the length of the animation, if it was changed.
		/// </summary>
		public void stopAnim()
		{
			pauseAnim();
			resetAnim();
			AnimLength = defaultAnimLength;
		}

		/// <summary>
		/// Stops advancing this animation, but keep the current frame.
		/// </summary>
		public void pauseAnim()
		{
			play = false;
			loop = false;
		}

		/// <summary>
		/// Resets animation to the first frame.
		/// </summary>
		public void resetAnim()
		{
			CurrentFrame = frames[0];
			frameIndex = 0;
		}

		public bool isPlaying()
		{
			return play;
		}

		private bool transition()
		{
			if ((frameIndex + 1) < frames.Count)
			{
				CurrentFrame = frames[frameIndex + 1];
				frameIndex++;
				return true;
			}
			//otherwise:
			resetAnim();
			return loop;
		}


	}
}
