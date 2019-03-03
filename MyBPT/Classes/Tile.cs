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
        int textureid;
        Texture2D texture;
        Vector2 position;
        Vector2 tempposition;
        Rectangle area;
        bool moving;
        bool highlighted;

        public Tile(int textureid, Texture2D texture, Vector2 position, Rectangle area) {
            this.textureid = textureid;
            this.texture = texture;
            this.position = position;
            this.tempposition = position;
            this.area = area;
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

        public void CheckIfReleased(TouchLocation tl) //because locationstate.released seems inconsistent
        {
            if (tl.State != TouchLocationState.Moved)
            {
                this.moving = false;

            }
        }

        //ACCESSORS


        /*public void MoveIfTouchIsHeld(GameWorld gameworld, SpriteBatch spriteBatch, TouchLocation tl) {
            area.Location = new Point((int)tempposition.X, (int)tempposition.Y);
            if (tl.State == TouchLocationState.Pressed && this.area.Contains(tl.Position)) {
                this.moving = true;

            }
            if (tl.State == TouchLocationState.Moved && moving) {
                this.tempposition = new Vector2(tl.Position.X - texture.Width/2, tl.Position.Y - texture.Height/2);
            }
            spriteBatch.Draw(texture, tempposition, Color.White);
            CheckIfReleased(tl);
        }
        */

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

        public int TextureID
        {
            get
            {
                return textureid;
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

    }
}