using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MyBPT.Classes
{
    class Bus
    {
        Texture2D bustexture;

        int worldsize;
        int positiononaxis;
        bool directionisX;
        bool visible;
        Vector2 position;

        public Bus(Texture2D bustexture, int positiononaxis, int worldsize, bool directionisX)
        {
            this.bustexture = bustexture;
            this.worldsize = worldsize;
            this.positiononaxis = positiononaxis;
            this.directionisX = directionisX;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(bustexture, position, Color.White);
        }

        public void UpdatePosition()
        {

        }
    }
}