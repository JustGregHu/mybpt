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
using Microsoft.Xna.Framework.Graphics;

namespace MyBPT.Classes
{
    class GameTextures
    {
        Dictionary<string, Texture2D> collection = new Dictionary<string, Texture2D>();
        
        public Dictionary<string, Texture2D> GetTextures()
        {
            return collection;
        }

        public void AddTexture(string newtextureid, Texture2D newtexture)
        {
            collection.Add(newtextureid,newtexture);
        }

    }
}