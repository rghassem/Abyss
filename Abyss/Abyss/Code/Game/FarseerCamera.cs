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
using Abyss.Code.Screen;

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
        public Rectangle Screen;
        World PhysicsWorld;
        DebugViewXNA Debug;
        GraphicsDevice Device;
        public SpriteBatch spriteBatch;

		//Lighting
		public List<LightSource> lightSources;
		Matrix lightView = Matrix.Identity;
		Vector2 lightPosition;

		//Quad
		Quad quad;
		Matrix QuadView, QuadProjection;
		BasicEffect quadEffect;
		VertexDeclaration quadVertexDecl;

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

			//Lighting stuff
			lightSources = new List<LightSource>();
			effect = content.Load<Effect>("Lighting");

			//Quad
			quad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, 2f, 2.0f);
			QuadView = Matrix.Identity;// Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero, Vector3.Up);
			QuadProjection = Matrix.Identity;// CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4.0f / 3.0f, 1, 500);

			quadEffect = new BasicEffect(Device);
			//quadEffect.EnableDefaultLighting();

			quadEffect.World = Matrix.Identity;
			quadEffect.View = QuadView;
			Matrix quadProjection = Matrix.CreateScale(1280, 720, 1) * Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero, Vector3.Down); ;
			effect.Parameters["Orthographic"].SetValue(quadProjection);
			quadEffect.TextureEnabled = true;

			quadVertexDecl = new VertexDeclaration(new VertexElement[]
				{
					new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
					new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
					new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
				}
			);
        }

        /// <summary>
        /// Add a sprite with a position and color to the Draw Queue. It will be drawn in the order
        /// that it was added, when the camera's draw() method is called.
        /// </summary>
        /// <param name="texture">The Sprite to draw</param>
        /// <param name="position">Where to draw to the top left corner of the object, in screen coords</param>
        /// <param name="color">The color parameter to pass to spriteBatch.draw()</param>
        public void record(Texture2D texture, Vector2 position, Color color, Rectangle? sourceRectangle,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
			if (texture == null) return;

			spriteBatch.Draw(texture, position * pixelsPerMeter, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
		public void record(Texture2D texture, Vector2 position, Color color)
		{
			if (texture == null) return;

			spriteBatch.Draw(texture, position * pixelsPerMeter, null,
							color, 0, Vector2.Zero, Vector2.One,
							SpriteEffects.None, 1);
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

		public void beginRecord() {
			spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, View);
		}

        public void endRecord()
        {
			spriteBatch.End();
		}

		public void drawDebug()
		{
            if (bDebugging)
			{
				Matrix PhysicsView = Matrix.CreateScale(pixelsPerMeter) * View *
					Matrix.CreateTranslation(-Screen.Width * .5f, -Screen.Height * .5f, 0f);
            
                Matrix Projection = Matrix.CreateOrthographic(Screen.Width, -Screen.Height, 0, 1);
                Debug.RenderDebugData(ref Projection, ref PhysicsView);
            }

        }

		/// <summary>
		/// Draws what the camera "recorded" this frame to the screen. Applies pixel shaders to renderTarget and
		/// draws it to the backbuffer.
		/// </summary>
		public void Draw(RenderTarget2D rt)
		{
			/*spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity);
			spriteBatch.Draw(rt, new Vector2(0, 0), null, Color.White);
			spriteBatch.End();*/

			/*VertexBuffer vb = new VertexBuffer(Device, VertexPositionColorTexture.VertexDeclaration,
				quad.Vertices.Length, BufferUsage.WriteOnly);
			vb.SetData(quad.Vertices);
			Device.SetVertexBuffer(vb);*/
			//vertexBuffer.SetData(vertices);
			//quadEffect.Begin();
			Device.Textures[0] = rt;

			//myVertexBuffer = new VertexBuffer(device, VertexPositionColorNormal.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			//myVertexBuffer.SetData(vertices);

			float radius = pixelsPerMeter; //hardcoding this for quick results
			//the center of the player character's position
			Vector2 center = UnitConverter.ToDisplayUnits(Subject.Position);
			//center.X -= radius/2;
			//center.Y += radius;
			center = convertToShaderSpace(center);
			

			List<Vector2> playerVertices = new List<Vector2>();
			List<Vector2> playerNormals = new List<Vector2>();
			Vector2 angle = Vector2.UnitX;

			Vector2 newVertex;
			Vector2 currentPoint;
			Matrix m;
			Vector2 n;
			const int numVerticesInCircle = 20;
			for (int i = 0; i < numVerticesInCircle; i++)
			{
				newVertex = new Vector2(1, 0);
				m = Matrix.CreateRotationZ(0.314159265f*i);
				newVertex = Vector2.Transform(newVertex, m);
				newVertex *= radius;
				newVertex = Vector2.Transform(newVertex, Matrix.CreateTranslation(center.X, center.Y, 0));
				playerVertices.Add(newVertex);
			}

			for (int i = 0; i < numVerticesInCircle; i++)
			{
				currentPoint = playerVertices[i];
				n = currentPoint - center;
				n.Normalize();
				playerNormals.Add(n);
			}

			List<bool> backFacing = new List<bool>(numVerticesInCircle);
			for (int i = 0; i < numVerticesInCircle; i++)
				backFacing.Add(false);
			for (int i = 0; i < numVerticesInCircle; i++)
			{
				currentPoint = playerNormals[i];
				n = lightPosition - currentPoint;
				n.Normalize();
				float lightAngle = Vector2.Dot(currentPoint, n);
				backFacing[i] = (lightAngle <= 0);
			}

			bool foundOne = false;
			Vector2[] edgeVertices = new Vector2[2];
			float[] pointIndices = new float[2];
			for (int i = 0; i < numVerticesInCircle; i++)
			{
				if (backFacing[i] != backFacing[(i + 1) % numVerticesInCircle])
					if (!foundOne)
					{
						edgeVertices[0] = playerVertices[i];
						pointIndices[0] = i;
						foundOne = true;
					}
					else
					{
						edgeVertices[1] = playerVertices[i];
						pointIndices[1] = i;
					}
			}

			Vector2 dir = (center - lightPosition);
			Vector2 vertex1;
			Vector2 vertex2;
			float length = dir.Length() + radius;
			effect.Parameters["distanceToShadowCaster"].SetValue(length);
			dir = (edgeVertices[0] - lightPosition);
			dir.Normalize();
			vertex1 = (dir * 1000);
			effect.Parameters["shadow_vertex1"].SetValue(vertex1);

			dir = (edgeVertices[1] - lightPosition);
			dir.Normalize();
			vertex2 = (dir * 1000);
			effect.Parameters["shadow_vertex2"].SetValue(vertex2);
			vertex1 = vertex1 - lightPosition;
			vertex2 = vertex2 - lightPosition;
			vertex2.Normalize();
			vertex1.Normalize();
			float tester = Vector2.Dot(vertex2, vertex1);

			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				Device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(
					PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indices, 0, 2);
			}
		}

        /// <summary>
        /// Allows the camera to update its View transform based on the position of the subject.
        /// Call during the game's update loop or scrolling etc. will not work.
        /// </summary>
        public void update()
        {
			if (Input.pickUpPressed() )
				bDebugging = !bDebugging;

            scrollView();
			updateLights();
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

				//Keep a seperate view matrix for lights
				Matrix ls = Matrix.CreateTranslation(-scrollX, scrollY, 0);
				lightView = Matrix.Multiply(lightView, ls);

                //transform the screen variable as well
                Screen = new Rectangle((int)(Screen.X + scrollX), (int)(Screen.Y + scrollY),
                    Screen.Width, Screen.Height);
            }
        }

		private void updateLights()
		{
			if (lightSources.Count > 0)
			{
				lightPosition = convertToShaderSpace(lightSources[0].Position);
				effect.Parameters["LightPosition"].SetValue(lightPosition);
			}
		}

		private Vector2 convertToShaderSpace(Vector2 v)
		{
			Vector2 result = Vector2.Transform(v, View);
			result.Y -= 720;
			result = Vector2.Reflect(result, Vector2.UnitX);
			return result;
		}

    }

	public struct Quad
	{
		public Vector3 Origin;
		public Vector3 UpperLeft;
		public Vector3 LowerLeft;
		public Vector3 UpperRight;
		public Vector3 LowerRight;
		public Vector3 Normal;
		public Vector3 Up;
		public Vector3 Left;

		public VertexPositionColorTexture[] Vertices;
		public int[] Indices;

		public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
		{
			Vertices = new VertexPositionColorTexture[4];
			Indices = new int[6];
			Origin = origin;
			Normal = normal;
			Up = up;

			// Calculate the quad corners
			Left = Vector3.Cross(normal, Up);
			Vector3 uppercenter = (Up * height / 2) + origin;
			UpperLeft = uppercenter + (Left * width / 2);
			UpperRight = uppercenter - (Left * width / 2);
			LowerLeft = UpperLeft - (Up * height);
			LowerRight = UpperRight - (Up * height);

			FillVertices();
		}

		private void FillVertices()
		{
			// Fill in texture coordinates to display full texture
			// on quad
			Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
			Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
			Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
			Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

			// Provide a normal for each vertex
			for (int i = 0; i < Vertices.Length; i++)
			{
				Vertices[i].Color = Color.White;
			}

			// Set the position and texture coordinate for each
			// vertex
			Vertices[0].Position = LowerLeft;
			Vertices[0].TextureCoordinate = textureLowerLeft;
			Vertices[1].Position = UpperLeft;
			Vertices[1].TextureCoordinate = textureUpperLeft;
			Vertices[2].Position = LowerRight;
			Vertices[2].TextureCoordinate = textureLowerRight;
			Vertices[3].Position = UpperRight;
			Vertices[3].TextureCoordinate = textureUpperRight;

			// Set the index buffer for each vertex, using
			// clockwise winding
			Indices[0] = 0;
			Indices[1] = 1;
			Indices[2] = 2;
			Indices[3] = 2;
			Indices[4] = 1;
			Indices[5] = 3;
		}
	}
}
