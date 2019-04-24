using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Microsoft.Xna.Framework;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using com.bitbull.meat;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes {
    class Hud {
        public Matrix transform;
        Viewport view;
        Vector2 position;
        Lerper lerper = new Lerper();

        public Hud (Viewport newView) {
            position = new Vector2(0, 0);
            view = newView;
        }

        public void Update(GameTime gameTime, TouchCollection tc) {
            transform = Matrix.CreateTranslation(new Vector3(-position.X - view.Width / 2, -position.Y - view.Height / 2, 0));
        }
    }
}