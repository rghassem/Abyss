#define ENABLE_GUI

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
using FarseerPhysics.DebugViewXNA;
using Abyss.Code.Game;
using Abyss.Code.Screen;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Troymium.Managed;
using System.Runtime.InteropServices;
using System.Reflection;
using Forms = System.Windows.Forms;
using System.Text;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;

using Abyss.Code.UserInterface.OSD;


namespace Abyss
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AbyssGame : Microsoft.Xna.Framework.Game
    {
        public static int ScreenWidth = 1280;
        public static int ScreenHeight = 720;

		#if ENABLE_GUI
		#region TroymiumInit
		
		[DllImport("user32.dll")]
		static extern int ToUnicode(
			uint wVirtKey, uint wScanCode, byte[] lpKeyState,
			[Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff,
			int cchBuff, uint wFlags
		);

		byte[] temporaryByteBuffer;
		Texture2D backingStore;
		Window webKit;
		int fadeDirection = 0;
		long? fadingSince = null;
		Context context;
		KeyboardState oldKB;
		KeyboardState newKB;
		MessageHook textInputHandler;

		MouseState oldMouseState;
		long lastKeystrokeTime = 0;
		#endregion
		#endif

        #region Static Members
        public static ContentManager Assets;
        public static SpriteBatch spriteBatch;
        public Random RNG = new Random();
        #endregion

        GraphicsDeviceManager graphics;
        //OSD osd;

        ScreenManager screenManager;

		public SpriteBatchRenderer renderer;

        public AbyssGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
           // Content.Load<Texture2D>("lud_desaturated_sprite");
            Assets = Content;

			// ADDED FOR SUPPORTING TROYMIUM
			#if ENABLE_GUI
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
			graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
			graphics.PreferredDepthStencilFormat = DepthFormat.Depth16;
			graphics.PreferMultiSampling = false;

			textInputHandler = new MessageHook(Window.Handle);
			textInputHandler.MessageReceived += (sender, e) =>
			{
				Forms.Message msg = textInputHandler.LastMessage;
				switch (msg.Msg)
				{
					case MessageHook.WM_KEYDOWN:
						webKit.KeyEvent(true, GetModifiers(), msg.WParam.ToInt32(), 0);
						break;
					case MessageHook.WM_KEYUP:
						webKit.KeyEvent(false, GetModifiers(), msg.WParam.ToInt32(), 0);
						break;
					case MessageHook.WM_CHAR:
						break;
				}
			};

			TroymiumNET.Init("Content");
			context = Context.Create();
			#endif // ENABLE_GUI

			// END OF TROYMIUM SUPPORT
            //osd = new OSD();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.ApplyChanges();
            Window.Title = "Abyss v0.2";

			IsMouseVisible = true; // Added for Troymium

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			screenManager = new ScreenManager(this);

			renderer = new SpriteBatchRenderer
			{
				GraphicsDeviceService = graphics
			};

			renderer.LoadContent(Content);

			base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
			screenManager.loadGameLevel();

			// ADDED FOR TROYMIUM
			#if ENABLE_GUI

			// We have to inject our teardown code here because if we let XNA's
			//  normal disposal handlers run, Chrome's message pump gets confused
			//  and hangs our process
			backingStore = new Texture2D(
				GraphicsDevice,
				GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color);

			// Create a temporary buffer for SetData/GetData
			temporaryByteBuffer = new byte[4 * GraphicsDevice.Viewport.Width * GraphicsDevice.Viewport.Height];

			webKit = new Troymium.Managed.Window(context);
			webKit.Paint += WebKit_Paint;
			webKit.StartLoading += WebKit_StartLoading;
			webKit.Load += WebKit_Load;
			webKit.NavigationRequested += delegate(Troymium.Managed.Window win, NavigationRequestedEventArgs arg)
			{
				if (arg.UserGesture) win.NavigateTo(arg.Url, arg.Referrer);
			};
			webKit.Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			webKit.Transparent = true;
			webKit.NavigateTo("file:///Content/AbyssUI.html");
			//Console.WriteLine("CURRENT DIRECTORY " + System.IO.Directory.GetCurrentDirectory());
			// END OF TROYMIUM SUPPORT
			#endif // ENABLE_GUI

            // TODO: use this.Content to load your game content here
 
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

			// BEGIN TROYMIUM SUPPORT
			#if ENABLE_GUI
			newKB = Keyboard.GetState();
			var newMouseState = Mouse.GetState();

			var scrollDelta = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;

			if ((newMouseState.X != oldMouseState.X) || (newMouseState.Y != oldMouseState.Y))
				webKit.MouseMoved(newMouseState.X, newMouseState.Y);

			if (scrollDelta != 0)
				webKit.MouseWheel(0, scrollDelta);

			if (newMouseState.LeftButton != oldMouseState.LeftButton)
				webKit.MouseButton(MouseButton.Left, newMouseState.LeftButton == ButtonState.Pressed);

			if (newMouseState.MiddleButton != oldMouseState.MiddleButton)
				webKit.MouseButton(MouseButton.Middle, newMouseState.MiddleButton == ButtonState.Pressed);

			if (newMouseState.RightButton != oldMouseState.RightButton)
				webKit.MouseButton(MouseButton.Right, newMouseState.RightButton == ButtonState.Pressed);

			var buffer = new StringBuilder();
			byte[] pressedKeys = new byte[256];

			long now = DateTime.UtcNow.Ticks;
			bool shouldRepeat = (now - lastKeystrokeTime) > TimeSpan.FromSeconds(0.15).Ticks;

			for (int i = 0; i < 255; i++)
			{
				var k = (Keys)i;
				bool wasDown = oldKB.IsKeyDown(k);
				bool isDown = newKB.IsKeyDown(k);
				if ((wasDown != isDown) || (isDown && shouldRepeat))
				{
					lastKeystrokeTime = now;
					webKit.KeyEvent(isDown, 0, i, 0);

					if (isDown)
					{
						Array.Clear(pressedKeys, 0, 256);

						foreach (var pk in newKB.GetPressedKeys())
						{
							int ik = (int)pk;

							if ((pk == Keys.LeftShift) || (pk == Keys.RightShift))
								ik = 0x0010;

							pressedKeys[ik] = 255;
						}

						int chars = ToUnicode((uint)i, 0, pressedKeys, buffer, 1, 0);
						if (chars > 0)
							webKit.TextEvent(buffer.ToString(0, chars));
					}
				}
			}
			oldMouseState = newMouseState;
			TroymiumNET.Update();
			oldKB = newKB;
			#endif // ENABLE_GUI
			// END OF TROYMIUM SUPPORT

            screenManager.update(gameTime);
            base.Update(gameTime); //updates all GameComponents (GameObjects)
			Input.updateInput();
        }

        public void addComponent(GameComponent gameObject)
        {
            Components.Add(gameObject);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            /*spriteBatch.Begin();
            // TODO: Add your drawing code here
            osd.Draw(gameTime);
            spriteBatch.End();*/

			screenManager.drawActiveScreen(gameTime);

			// ADDED FOR TROYMIUM
			#if ENABLE_GUI	
			byte a = 255;
			long now = DateTime.UtcNow.Ticks;
			long elapsed = now - fadingSince.GetValueOrDefault(0);
			if (fadeDirection > 0)
			{
				a = (byte)MathHelper.Clamp(255 * elapsed / TimeSpan.FromSeconds(0.25).Ticks, 0, 255);
			}
			else if (fadeDirection < 0)
			{
				a = (byte)(255 - MathHelper.Clamp(255 * elapsed / TimeSpan.FromSeconds(0.25).Ticks, 0, 255));
			}

			spriteBatch.Begin();

			// Draw GUI.
			spriteBatch.Draw(backingStore, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Color(255, 255, 255, a));
			spriteBatch.End();
			#endif
			// END OF TROYMIUM SUPPORT
			

            base.Draw(gameTime);
        }

		#if ENABLE_GUI
		#region troymiumMethods
		// START OF TROYMIUM METHODS
		protected KeyModifier GetModifiers() {
			KeyboardState kb = Keyboard.GetState();
			KeyModifier result = 0;

			if (kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt)) result |= KeyModifier.ALT_MOD;
			if (kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl)) result |= KeyModifier.CONTROL_MOD;
			if (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift)) result |= KeyModifier.SHIFT_MOD;

			return result;
		}

		private void WebKit_StartLoading(Window window, UrlEventArgs args)
		{
			fadeDirection = -1;
			fadingSince = DateTime.UtcNow.Ticks;
		}

		private void WebKit_Load(Window window)
		{
			if (fadeDirection == -1)
			{
				fadeDirection = 1;
				fadingSince = DateTime.UtcNow.Ticks;
			}
		}

		private void WebKit_Paint(Window window, PaintEventArgs args)
		{
			if (args.SourceBufferRect.Width <= 0 || args.SourceBufferRect.Height <= 0)
			{
				return;
			}
			int sourceBufferSize = args.SourceBufferRect.Width * args.SourceBufferRect.Height * args.SourceBufferRect.BytePerPixel;

			GraphicsDevice.Textures[0] = null;

			if (args.X != 0 || args.Y != 0)
			{
				var sourceRect = new Rectangle(args.ScrollRect.Left, args.ScrollRect.Top, args.ScrollRect.Width, args.ScrollRect.Height);
				var destRect = sourceRect;
				destRect.X += args.X;
				destRect.Y += args.Y;
				
				var overlap = Rectangle.Intersect(destRect, sourceRect);

				// We need to handle scrolling to the left
				if (destRect.Left < 0)
				{
					sourceRect.X -= destRect.Left;
					destRect.X = 0;
				}
				// And upward
				if (destRect.Top < 0)
				{
					sourceRect.Y -= destRect.Top;
					destRect.Y = 0;
				}

				destRect.Width = sourceRect.Width = overlap.Width;
				destRect.Height = sourceRect.Height = overlap.Height;

				if ((sourceRect.Width > 0) && (sourceRect.Height > 0))
				{
					// Copy the scrolled portion out of the texture into our temporary buffer
					backingStore.GetData<byte>(0, sourceRect, temporaryByteBuffer, 0, sourceRect.Width * sourceRect.Height * 4);
				
					// And then copy it back into the new location
					backingStore.SetData<byte>(0, destRect, temporaryByteBuffer, 0, destRect.Width * destRect.Height * 4);
				}
			}

			for (int i = 0; i < args.CopyRects.Length; i++)
			{
				var temptRectDest = new Rectangle(args.CopyRects[i].Left, args.CopyRects[i].Top, args.CopyRects[i].Width, args.CopyRects[i].Height);
				var temptRectSource = new Rect(args.CopyRects[i].Left - args.SourceBufferRect.Left, args.CopyRects[i].Top - args.SourceBufferRect.Top, args.CopyRects[i].Width, args.CopyRects[i].Height);
				TroymiumNET.CopyArea(args.SourceBuffer, args.SourceBufferRect.Stride, temptRectSource, temporaryByteBuffer);				

				// Ugh. Why doesn't SetData accept a pointer? Terrible.
				var copySize = temptRectSource.Width * temptRectSource.Height * 4;


				for (int j = 0; j < copySize; j += 4) { 
					byte temp = temporaryByteBuffer[j]; 
					temporaryByteBuffer[j] = temporaryByteBuffer[j + 2]; 
					temporaryByteBuffer[j + 2] = temp; 
				}
				
				backingStore.SetData<byte>(0, temptRectDest, temporaryByteBuffer, 0, copySize);
			}
		}

		public void Teardown()
		{
			webKit.Dispose();
			webKit = null;
			context.Dispose();
			TroymiumNET.Destroy();
		}
		// END OF TROYMIUM METHODS
		#endregion 
		#endif // ENABLE_GUI
	}
}
