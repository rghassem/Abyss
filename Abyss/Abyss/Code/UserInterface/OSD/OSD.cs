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
using Abyss.Code.Game;

namespace Abyss.Code.UserInterface.OSD
{
    class OSD
    {
        Box healthDisplay;
        Box itemDisplay;
        
        public OSD()
        {
            healthDisplay = new Box(20, 20, 200, 100);
            itemDisplay = new Box(1000, 600, 300, 150);
        }

        public void LoadContent()
        {
            healthDisplay.LoadTextures();
            itemDisplay.LoadTextures();
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime, PlayerCharacter pc)
        {
            healthDisplay.Render("Health Display\n"+pc.Health);
            itemDisplay.Render("Item Display");
        }
    }
}
