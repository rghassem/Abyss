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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Abyss;

namespace Abyss.Code.UserInterface.OSD
{
    /// <summary>
    /// Just a filler class for the moment.  Just used for the mockup
    /// will include a better framework for the real thing.
    /// Same code that's used by the CS113 game I created
    /// -Shawn
    /// </summary>
    class Box
    {
        SpriteFont font;
        Texture2D bNorth;
        Texture2D bSouth;
        Texture2D bEast;
        Texture2D bWest;
        Texture2D background;
        Texture2D cNE;
        Texture2D cSE;
        Texture2D cNW;
        Texture2D cSW;
        protected int x;
        protected int y;
        protected int width;
        protected int height;
        Color transparentColor = new Color(255, 255, 255, 240);

        public void LoadTextures()
        {
            bNorth = AbyssGame.Assets.Load<Texture2D>("Box1\\bNorth");
            bSouth = AbyssGame.Assets.Load<Texture2D>("Box1\\bSouth");
            bEast = AbyssGame.Assets.Load<Texture2D>("Box1\\bEast");
            bWest = AbyssGame.Assets.Load<Texture2D>("Box1\\bWest");
            background = AbyssGame.Assets.Load<Texture2D>("Box1\\background");
            cNE = AbyssGame.Assets.Load<Texture2D>("Box1\\cNE");
            cSE = AbyssGame.Assets.Load<Texture2D>("Box1\\cSE");
            cNW = AbyssGame.Assets.Load<Texture2D>("Box1\\cNW");
            cSW = AbyssGame.Assets.Load<Texture2D>("Box1\\cSW");
            font = AbyssGame.Assets.Load<SpriteFont>("Fonts\\Default");
        }

        public Box(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        void DrawBorders()
        {
            for (int i = cNW.Width; i < (this.width - cNE.Width); ++i)
            {
                // draw north border
                AbyssGame.spriteBatch.Draw(bNorth, new Vector2(x + i, y), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            }
            for (int i = cSW.Width; i < (this.width - cSE.Width); ++i)
            {
                // draw south border
                AbyssGame.spriteBatch.Draw(bSouth, new Vector2(x + i, y + height - bSouth.Height), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            }
            for (int i = cNW.Height; i < (this.height - cSW.Height); ++i)
            {
                // draw west border
                AbyssGame.spriteBatch.Draw(bWest, new Vector2(x, y + i), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            }
            for (int i = cNE.Height; i < (this.height - cSE.Height); ++i)
            {
                // draw east border
                AbyssGame.spriteBatch.Draw(bEast, new Vector2(x + width - bEast.Width, y + i), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            }
        }

        public void Render(String text)
        {
            float xScalar = ((float)width - (float)bWest.Width - (float)bEast.Width) / (float)background.Width;
            float yScalar = ((float)height - (float)bSouth.Height - (float)bNorth.Height) / (float)background.Height;
            AbyssGame.spriteBatch.Draw(background, new Vector2(x + bWest.Width, y + bNorth.Height), null, transparentColor, 0, Vector2.Zero, new Vector2(xScalar, yScalar), SpriteEffects.None, 0);

            this.DrawBorders();
            AbyssGame.spriteBatch.Draw(cNW, new Vector2(x, y), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            AbyssGame.spriteBatch.Draw(cNE, new Vector2(x + width - cNE.Width, y), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            AbyssGame.spriteBatch.Draw(cSW, new Vector2(x, y + height - cSW.Height), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            AbyssGame.spriteBatch.Draw(cSE, new Vector2(x + width - cSE.Width, y + height - cSE.Height), null, transparentColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            AbyssGame.spriteBatch.DrawString(font, text, new Vector2(getVisibleX() + 10, getVisibleY() + 10), Color.Green);
        }

        public int getVisibleX()
        {
            return x + bWest.Width;
        }

        public int getVisibleY()
        {
            return y + bNorth.Height;
        }
    }
}
