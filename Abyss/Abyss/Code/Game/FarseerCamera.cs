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
using FarseerPhysics.Dynamics;
using FarseerPhysics.DebugViewXNA;

namespace Abyss.Code.Game
{
    public class FarseerCamera
    {

        /*
         *TODO:
         *ScrollOffCenter
         *Pixel Coordinates
         *Summery for Contructor
        */
        private Rectangle Screen;
        World PhysicsWorld;
        DebugViewXNA Debug;
        GraphicsDevice Device;
        SpriteBatch spriteBatch;

        private readonly int pixelsPerMeter;

        /// <summary>
        /// The distance the Subject can get to the edge of the screen before the screen scrolls.
        /// If in Scroll mode its the disance on any side.
        /// If in ScrollOffCenter mode its the distance in front.
        /// </summary>
        public int subjetDistanceToScreenEdge {get; set;}

        /// <summary>
        /// The FarseerCamera's ViewMode determines how the Camera moves with the Subject.
        /// CenterOnSubject: Subject is center screen at all times.
        /// Scroll: Screen moves to keep up with Subject when he moves within [TODO] [PIXELS? MKS?] of the edge of the screen.
        /// ScrollOffCenter: Distance maintained from the edge of the screen is greater in front of the subject than in back.
        /// </summary>
        public enum ViewMode
        {
            CenterOnSubject,
            Scroll,
            ScrollOffCenter
        }
        /// <summary>
        /// The current ViewMode
        /// </summary>
        public ViewMode viewMode {get; set;}


        //All objects in DrawQueue are drawn when draw() is called.
        private Queue<drawObjectData> DrawQueue = new Queue<drawObjectData>(10); //random number. 

        //SpriteBatch Parameters
        SpriteSortMode sortMode { get; set; }
        SamplerState samplerState { get; set; }
        BlendState blendState { get; set; }
        DepthStencilState depthStencilState { get; set; }
        RasterizerState rasterizerState { get; set; }
        Effect effect { get; set; } 

        /// <summary>
        /// Set to true to enable debug mode for Farseer Physics Engine Bodies.
        /// </summary>
        public bool bDebugging { get; set; }

        /// <summary>
        /// Whether to use MKS (Farseer's coordinate system) when drawing objects, or
        /// pixel coordinates. 
        /// </summary>
        public bool bUseMKSCoordinates { get; set; }

        /// <summary>
        /// The transform the Camera applies to everything it draws. Accessor is public, but it shouldn't ever
        /// really be needed. To Draw something using the Camera's tranform, use the Camera's record method.
        /// </summary>
        public Matrix View { get; private set; }

        /// <summary>
        /// The Camera's target. If its not null, the camera will scroll with the subject. How is determined
        /// by the ViewMode.
        /// </summary>
        public GameObject Subject { get; set; }

//Methods for translating between Farseer MKS coordinates and pixel coordinates.
        private float screenToMKS(float screenCoord)
        {
            return screenCoord / pixelsPerMeter;
        }
        private Vector2 screenToMKS(Vector2 screenCoords)
        {
            return new Vector2(screenToMKS(screenCoords.X), screenToMKS(screenCoords.Y));
        }

        private float MKSToScreen(float mksCoord)
        {
            return mksCoord * pixelsPerMeter;
        }
        private Vector2 MKSToScreen(Vector2 mksCoords)
        {
            return new Vector2(MKSToScreen(mksCoords.X), MKSToScreen(mksCoords.Y));
        }

        public FarseerCamera(ref SpriteBatch SpriteBacth, ContentManager content, GraphicsDevice device, 
            int ScreenWidth, int ScreenHeight, ref World world, int pixsPerMeter = 50)
        {
            Screen = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
            PhysicsWorld = world;
            Debug = new DebugViewXNA(PhysicsWorld);
            
            spriteBatch = SpriteBacth;
            Device = device;
            pixelsPerMeter = pixsPerMeter;

            //Initialize spriteBatch parmeters to defaults:
            sortMode = SpriteSortMode.Deferred;
            blendState = BlendState.AlphaBlend;
            samplerState = SamplerState.LinearClamp;
            depthStencilState = DepthStencilState.Default; 
            rasterizerState = RasterizerState.CullCounterClockwise;
            effect = null;
            View = Matrix.Identity;

            bDebugging = true;
            bUseMKSCoordinates = true;

            DebugViewXNA.LoadContent(Device, content);
        }

        /// <summary>
        /// Add a sprite with a position and color to the Draw Queue. It will be drawn in the order
        /// that it was added, when the camera's draw() method is called.
        /// </summary>
        /// <param name="texture">The Sprite to draw</param>
        /// <param name="position">Where to draw to the top left corner of the object, in screen coords</param>
        /// <param name="color">The color parameter to pass to spriteBatch.draw()</param>
        public void record(Texture2D texture, Vector2 position, Color color, Nullable<Rectangle> sourceRectangle,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
			DrawQueue.Enqueue(new drawObjectData(texture, position, color, sourceRectangle, rotation, origin,
				scale, effects, layerDepth));
        }
		public void record(Texture2D texture, Vector2 position, Color color)
		{
			DrawQueue.Enqueue(new drawObjectData(texture, position, color, null, 0, new Vector2(0,0),
				new Vector2(1,1), SpriteEffects.None, 1));
		}
        public void record(Texture2D texture, Vector2 position)
        {
            record(texture, position, Color.White, null, 0, new Vector2(0,0), new Vector2 (1,1),
				SpriteEffects.None, 0);
        }
        public void record(GameObject gameObject)
        {
            record(gameObject.Sprite, gameObject.Position, Color.White);
        }

        
        public void draw()
        {
                
            float aspect = (float)Screen.Width / Screen.Height;
            Matrix scale = Matrix.CreateScale(pixelsPerMeter);
            Matrix PhysicsView = scale * View *
                Matrix.CreateTranslation(-Screen.Width * .5f, -Screen.Height * .5f, 0f);
            
            spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, View);
                while(DrawQueue.Count > 0)
                {
                    drawObjectData currentObject = DrawQueue.Dequeue();
                    if (currentObject.texture != null)
                    {
                        //Tranform the sprite's position to line up with its physics body
                        Vector2 screenPos = Vector2.Transform(currentObject.position, scale);

						//not needed so long as sprite's origin is at its center.
                        //screenPos.X -= (currentObject.texture.Width / 2);
                        //screenPos.Y -= (currentObject.texture.Height / 2);

                        spriteBatch.Draw(currentObject.texture, screenPos, currentObject.sourceRectangle,
							currentObject.color, currentObject.rotation, currentObject.origin, currentObject.scale,
							currentObject.effects,currentObject.layerDepth);
                    }
                }
            spriteBatch.End();

            if (bDebugging)
            {
                Matrix Projection = Matrix.CreateOrthographic((Screen.Height * aspect), -Screen.Height, 0, 1);
                Debug.RenderDebugData(ref Projection, ref PhysicsView);
            }

        }


        /// <summary>
        /// Allows the camera to update its View transform based on the position of the subject.
        /// Call during the game's update loop or scrolling etc. will not work.
        /// </summary>
        public void update()
        {
            
            scrollView();
        }

        private void scrollView()
        {
            int distanceToMaintain;
            if(viewMode == ViewMode.CenterOnSubject)
                distanceToMaintain = Screen.Width;
            else distanceToMaintain = subjetDistanceToScreenEdge;

            int charCenterX = (int)Math.Round( MKSToScreen(Subject.Position.X)  );
            charCenterX -= distanceToMaintain / 2;
            int charCenterY = (int)Math.Round(  MKSToScreen(Subject.Position.Y)  );
            charCenterY -= distanceToMaintain / 2;
            Rectangle positionBuffer = new Rectangle(charCenterX, charCenterY, distanceToMaintain, distanceToMaintain);
            if (!Screen.Contains(positionBuffer))
            {
                float scrollX = 0;
                float scrollY = 0;
                if (Screen.Left > positionBuffer.Left)
                    scrollX = positionBuffer.Left - Screen.Left;
                if (Screen.Right < positionBuffer.Right)
                    scrollX = positionBuffer.Right - Screen.Right;
                if (Screen.Top > positionBuffer.Top)
                    scrollY = positionBuffer.Top - Screen.Top;
                if (Screen.Bottom < positionBuffer.Bottom)
                    scrollY = positionBuffer.Bottom - Screen.Bottom;

                Matrix scroll = Matrix.CreateTranslation(-scrollX, -scrollY, 0);
                // Matrix scroll = Matrix.CreateTranslation(100, 10, 0);
                View = Matrix.Multiply(View, scroll);
                //transform the screen variable as well
                Screen = new Rectangle((int)(Screen.X + scrollX), (int)(Screen.Y + scrollY),
                    Screen.Width, Screen.Height);
            }
        }


        /// <summary>
        /// Internal struct used to store data on objects to be drawn. They are all drawn at once when
        /// draw() is called, in the order they were added to the Draw Queue. Call record() to
        /// add an object to the Draw Queue.
        /// </summary>
        internal struct drawObjectData
        {
            public Texture2D texture;
            public Vector2 position;
            public Color color;

			public Nullable<Rectangle> sourceRectangle;
			public float rotation;
			public Vector2 origin;
			public Vector2 scale;
			public SpriteEffects effects;
			public float layerDepth;

            public drawObjectData(Texture2D t, Vector2 p, Color c, Nullable<Rectangle> rec, float rot,
				Vector2 orig, Vector2 s, SpriteEffects fx, float ld)
            {
                texture = t;
                position = p;
                color = c;
				sourceRectangle = rec;
				rotation = rot;
				origin = orig;
				scale = s;
				effects = fx;
				layerDepth = ld;
            }
        }

    }
}
