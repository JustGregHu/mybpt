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
    class Road
    {
        Texture2D texture;
        Vector2 tileposition;
        Point coordinates;

        public Point Coordinates { get => coordinates; set => coordinates = value; }

        public Road(Dictionary<string, Texture2D> texturecollection, string textureid, GameWorld gameWorld,Point coordinates)
        {
            this.texture = texturecollection[textureid];
            this.coordinates = coordinates;
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, tileposition, Color.White);
        }
    }
}