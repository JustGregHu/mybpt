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
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes {
    class Tile {
        int height;
        Texture2D texture;
        Vector2 position;
        Vector2 tempposition;
        Rectangle area;
        bool moving;
        bool highlighted;

        public Tile(int height, Texture2D texture, Vector2 position, Rectangle area) {
            this.height = height;
            this.texture = texture;
            this.position = position;
            this.tempposition = position;
            this.Area = area;
            this.moving = false;
            highlighted = false;
        }

        public void Draw(SpriteBatch spriteBatch) { //draws the sprite, highlight and effects must go here
            if (highlighted)
            {
                spriteBatch.Draw(new Texture2D(spriteBatch.GraphicsDevice, 100, 100), position, Color.White);
            }
            else
            {
                spriteBatch.Draw(texture, position, Color.White);
            }
        }


        //ACCESSORS

        public Vector2 Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }

        public bool Moving {
            get {
                return moving;
            }
            set {
                moving = value;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public bool Highlighted
        {
            get
            {
                return highlighted;
            }
            set
            {
                highlighted = value;
            }
        }

        public Texture2D Texture { get => texture; set => texture = value; }
        public Rectangle Area { get => area; set => area = value; }
    }
}