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
        Texture2D texture;
        Vector2 position;
        Rectangle area;
        bool moving;

        public Tile(Texture2D texture, Vector2 position, Rectangle area) {
            this.texture = texture;
            this.position = position;
            this.area = area;
            this.moving = false;
        }


        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public void TouchHoldInitiate() {

        }

        public void TouchHoldEnd() {

        }


        public void MoveIfTouchIsHeld(SpriteBatch spriteBatch, TouchLocation tl) {
            area.Location = new Point((int)position.X, (int)position.Y);
            if (tl.State == TouchLocationState.Pressed && this.area.Contains(tl.Position)) {
                this.moving = true;

            }
            if (tl.State == TouchLocationState.Moved && moving) {
                this.position = new Vector2(tl.Position.X - texture.Width/2, tl.Position.Y - texture.Height/2);
            }
            if (tl.State == TouchLocationState.Released) {
                this.moving = false;
            }
            spriteBatch.Draw(texture, position, Color.White);
        }

        public bool IsMoving() {
            return moving;
        }

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


    }
}