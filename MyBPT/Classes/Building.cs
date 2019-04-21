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
    class Building
    {
        int type;
        Texture2D texture;
        Vector2 tileposition;
        Point coordinates;
        bool highlighted;
        Button demolishbutton;
        Tile highlighttile;
        int influenceamount;
        int level;

        public Button DemolishButton { get => demolishbutton; set => demolishbutton = value; }
        public Point Coordinates { get => coordinates; set => coordinates = value; }
        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public int Influenceamount { get => influenceamount; set => influenceamount = value; }

        public Building(Dictionary<string, Texture2D> texturecollection, GameWorld gameWorld, Point coordinates, int type, int level)
        {
            this.type = type;
            switch (level)
            {
                case 1:
                    switch (type)
                    {
                        case 0:
                            this.texture = texturecollection["building_residential1"];
                            break;
                        case 1:
                            this.texture = texturecollection["building_commercial1"];
                            break;
                        case 2:
                            this.texture = texturecollection["building_industrial1"];
                            break;
                        default: 
                            break;
                    }
                    break;
                case 2:
                    switch (type)
                    {
                        case 0:
                            this.texture = texturecollection["building_residential2"];
                            break;
                        case 1:
                            this.texture = texturecollection["building_commercial2"];
                            break;
                        case 2:
                            this.texture = texturecollection["building_industrial2"];
                            break;
                        default:
                            
                            break;
                    }
                    break;
                default:
                    break;
            }
            switch (type)
            {
                case 0:
                    SetInfluenceAmount(50);
                    break;
                case 1: SetInfluenceAmount(100);
                    break;
                case 2: SetInfluenceAmount(125);
                    break;
                default:
                    break;
            }
            this.coordinates = coordinates;
            demolishbutton = new Button(new Vector2(550, 50), texturecollection["demolishmenu_clear"]);
            highlighttile = new Tile(0, texturecollection["highlight_lightblue"], tileposition, new Rectangle(tileposition.ToPoint(), new Point(200, 100)));
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            ButtonVisibility_NoSelection();
        }

        public void SetInfluenceAmount(int cost)
        {
            influenceamount = cost+ level*(cost * type);
        }

        public String GetTypeString(int whattype)
        {
            switch (whattype)
            {
                case 0:return "Residential";
                case 1:return "Commercial";
                case 2:return "Industrial";
                default: return "Undefined";
            }
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