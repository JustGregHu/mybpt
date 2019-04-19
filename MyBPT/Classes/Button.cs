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
        Texture2D texture;
        SpriteFont text;

        public bool Visible { get => visible; set => visible = value; }

        public Button(Vector2 position, Texture2D texture) {
            highlighted = false;
            Visible = true;
            this.texture = texture;

            this.position = position;
            area = new Rectangle(position.ToPoint(),new Point(texture.Width, texture.Height));

        }

        public void Draw(SpriteBatch spriteBatch) {
            if (Visible) {
                spriteBatch.Draw(texture, new Vector2(position.X, position.Y), Color.White);
            }
        }

        public void DrawWithRotation(SpriteBatch spriteBatch, float angle)
        {
            if (Visible)
            {
                Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
                Rectangle destinationRectangle = new Rectangle((int)position.X,(int)position.Y, texture.Width, texture.Height);
                destinationRectangle.X += destinationRectangle.Width / 2;
                destinationRectangle.Y += destinationRectangle.Height / 2;
                spriteBatch.Draw(texture, destinationRectangle, null, Color.White, angle, origin, SpriteEffects.None, 0);
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