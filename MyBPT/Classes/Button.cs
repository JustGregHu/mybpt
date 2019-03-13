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
    class Button {
        Vector2 position;
        Rectangle area;
        bool highlighted;
        bool visible;
        //Action activateevent;
        Texture2D texture;
        SpriteFont text;
       


        public Button(Vector2 position, Texture2D texture) {
            highlighted = false;
            visible = true;
            this.texture = texture;

            this.position = position;
            //this.activateevent = activate;
            area = new Rectangle(position.ToPoint(),new Point(texture.Width, texture.Height));

        }

        /*
        public void Activate() {
            activateevent();
        }
        */

        public void Draw(SpriteBatch spriteBatch, Camera camera) {
            if (visible) {
                spriteBatch.Draw(texture, new Vector2(camera.Position.X+position.X, camera.Position.Y + position.Y), Color.White);
            }
        }

        public bool IsTapped(TouchLocation tl) {
            if (tl.State == TouchLocationState.Pressed && this.area.Contains(tl.Position)) {
                return true;

            }
            return false;
        }

        public bool IsHeld(TouchLocation tl) {
            if (tl.State == TouchLocationState.Moved && this.area.Contains(tl.Position)) {
                return true;

            }
            return false;
        }
    }
}