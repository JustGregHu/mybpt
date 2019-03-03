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
        Dictionary<int, Texture2D> collection = new Dictionary<int, Texture2D>();
        
        public Dictionary<int, Texture2D> GetTextures()
        {
            return collection;
        }

        public void AddTexture(int newtextureid, Texture2D newtexture)
        {
            collection.Add(newtextureid,newtexture);
        }

    }
}