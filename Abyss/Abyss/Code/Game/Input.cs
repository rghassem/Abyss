using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Abyss.Code.Game
{
	public class Input
	{
		static KeyboardState OldKeyboard;
		static GamePadState OldGamepad;

		const float threshold = 0.4f;

		//Key and button mappings
		const Keys useKey1 = Keys.LeftControl;
		const Keys useKey2 = Keys.RightControl;
		const Buttons useButton = Buttons.B;

		const Keys jumpKey = Keys.Space;
		const Buttons jumpButton = Buttons.A;

		const Keys pickUpKey = Keys.E;
		const Buttons pickUpButton = Buttons.X;

		const Keys invKey = Keys.Enter;
		const Buttons invButton = Buttons.Y;

		const Keys pauseKey = Keys.Escape;
		const Buttons pauseButton = Buttons.Start;

		/// <summary>
		/// True if Down has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool downPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(Keys.Down) &&
				!OldKeyboard.IsKeyDown(Keys.Down)) ||
				(Keyboard.GetState().IsKeyDown(Keys.S) &&
				!OldKeyboard.IsKeyDown(Keys.S))||
				(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -threshold &&
				!(OldGamepad.ThumbSticks.Left.Y < -threshold));
		}
		/// <summary>
		/// True if Up has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool upPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(Keys.Up) &&
				!OldKeyboard.IsKeyDown(Keys.Up)) ||
				(Keyboard.GetState().IsKeyDown(Keys.W) &&
				!OldKeyboard.IsKeyDown(Keys.W))||
				(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > threshold &&
				!(OldGamepad.ThumbSticks.Left.Y > threshold));
		}
		/// <summary>
		/// True if Right has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool rightPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(Keys.Right) &&
				!OldKeyboard.IsKeyDown(Keys.Right)) ||
				(Keyboard.GetState().IsKeyDown(Keys.D) &&
				!OldKeyboard.IsKeyDown(Keys.D))||
				(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > threshold &&
				!(OldGamepad.ThumbSticks.Left.X > threshold));
		}
		/// <summary>
		/// True if Left has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool leftPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(Keys.Left) &&
				!OldKeyboard.IsKeyDown(Keys.Left)) ||
				(Keyboard.GetState().IsKeyDown(Keys.A) &&
				!OldKeyboard.IsKeyDown(Keys.A)) ||
				(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < -threshold &&
				!(OldGamepad.ThumbSticks.Left.X < -threshold));
		}
		/// <summary>
		/// True if Jump has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool jumpPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(jumpKey) &&
				!OldKeyboard.IsKeyDown(jumpKey)) ||
				(GamePad.GetState(PlayerIndex.One).IsButtonDown(jumpButton) &&
				!OldGamepad.IsButtonDown(jumpButton));
		}
		/// <summary>
		/// True if Use has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool usePressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(useKey1) &&
				!OldKeyboard.IsKeyDown(useKey1)) ||
				(Keyboard.GetState().IsKeyDown(useKey2) &&
				!OldKeyboard.IsKeyDown(useKey2)) ||
				(GamePad.GetState(PlayerIndex.One).IsButtonDown(useButton) &&
				!OldGamepad.IsButtonDown(useButton));
		}
		/// <summary>
		/// True if Pick Up has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool pickUpPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(pickUpKey) &&
				!OldKeyboard.IsKeyDown(pickUpKey)) ||
				(GamePad.GetState(PlayerIndex.One).IsButtonDown(pickUpButton) &&
				!OldGamepad.IsButtonDown(pickUpButton));
		}
		/// <summary>
		/// True if the inventory button has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool invPressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(invKey) &&
				!OldKeyboard.IsKeyDown(invKey)) ||
				(GamePad.GetState(PlayerIndex.One).IsButtonDown(invButton) &&
				!OldGamepad.IsButtonDown(invButton));
		}
		/// <summary>
		/// True if the pause button has just been pressed (not held down since last frame)
		/// </summary>
		/// <returns></returns>
		public static bool pausePressed()
		{
			return
				(Keyboard.GetState().IsKeyDown(pauseKey) && 
				!OldKeyboard.IsKeyDown(pauseKey)) ||
				(GamePad.GetState(PlayerIndex.One).IsButtonDown(pauseButton) && 
				!OldGamepad.IsButtonDown(pauseButton));
		}

		/// <summary>
		/// True if Down is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isDownHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(Keys.Down) ||
				Keyboard.GetState().IsKeyDown(Keys.S) ||
				GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -threshold;
		}
		/// <summary>
		/// True if Up is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isUpHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(Keys.Up) ||
				Keyboard.GetState().IsKeyDown(Keys.W) ||
				GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > threshold;
		}
		/// <summary>
		/// True if Right is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isRightHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(Keys.Right) ||
				Keyboard.GetState().IsKeyDown(Keys.D) ||
				GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > threshold;
		}
		/// <summary>
		/// True if Left is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isLeftHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(Keys.Left) ||
				Keyboard.GetState().IsKeyDown(Keys.A) ||
				GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < -threshold;
		}
		/// <summary>
		/// True if Jump is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isJumpHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(jumpKey) ||
				GamePad.GetState(PlayerIndex.One).IsButtonDown(jumpButton);
		}
		/// <summary>
		/// True if Use is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isUseHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(useKey1) ||
				Keyboard.GetState().IsKeyDown(useKey2) ||
				GamePad.GetState(PlayerIndex.One).IsButtonDown(useButton);
		}
		/// <summary>
		/// True if PickUp is currently held down.
		/// </summary>
		/// <returns></returns>
		public static bool isPickUpHeld()
		{
			return
				Keyboard.GetState().IsKeyDown(pickUpKey) ||
				GamePad.GetState(PlayerIndex.One).IsButtonDown(pickUpButton);
		}


		/// <summary>
		/// Saves the inputs this frame to be checked against next frame.
		/// Should be called after all game update logic is finished.
		/// </summary>
		public static void updateInput()
		{
			OldKeyboard = Keyboard.GetState();
			OldGamepad = GamePad.GetState(PlayerIndex.One);
		}


	}
}
