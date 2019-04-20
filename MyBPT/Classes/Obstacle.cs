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
    class Obstacle
    {
        Texture2D texture;
        Vector2 tileposition;
        Point coordinates;
        bool highlighted;
        int cost;
        Button demolishbutton;
        Tile highlighttile;

        public Button DemolishButton { get => demolishbutton; set => demolishbutton = value; }
        public Point Coordinates { get => coordinates; set => coordinates = value; }
        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public int Cost { get => cost; set => cost = value; }

        public Obstacle(Dictionary<string,Texture2D> texturecollection, string textureid, GameWorld gameWorld,Point coordinates,int cost)
        {

            this.texture = texturecollection[textureid];
            this.cost = cost;
            this.coordinates = coordinates;
            demolishbutton = new Button(new Vector2(550, 50), texturecollection["demolishmenu_clear"]);
            highlighttile = new Tile(0, texturecollection["highlight_lightblue"], tileposition, new Rectangle(tileposition.ToPoint(), new Point(200, 100)));
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            ButtonVisibility_NoSelection();
        }

        public void CheckIfHighlighted(Point coordinatetocheck)
        {
            if (coordinatetocheck == coordinates)
            {
                highlighted = true;
            }
            else
            {
                highlighted = false;
            }
        }

        private void ButtonVisibility_NoSelection()
        {
            DemolishButton.Visible = false;

        }
        private void ButtonVisibility_Highlighted()
        {
            DemolishButton.Visible = true;
        }

        private Vector2 DisplayPosition()
        {
            return new Vector2((tileposition.X + 150) - (texture.Width), (tileposition.Y + 75) - (texture.Height));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (highlighted)
            {
                ButtonVisibility_Highlighted();
            }
            else
            {
                ButtonVisibility_NoSelection();
            }
            spriteBatch.Draw(texture, DisplayPosition(), Color.White);
        }
        public void DrawButtons(SpriteBatch spriteBatchHud)
        {
            DemolishButton.Draw(spriteBatchHud);
        }
    }
}