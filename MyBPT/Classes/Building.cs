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

namespace MyBPT.Classes {
    class Building {
        Texture2D texture;
        Vector2 tileposition;
        bool highlighted;
        bool moving;
        int effectradius;
        String description;
        int cost;
        bool isterminus;
        bool visible = true;
        Tile highlighttile;
        Point coordinates;
        int type;

        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public bool Isterminus { get => isterminus; set => isterminus = value; }
        public int Effectradius { get => effectradius; set => effectradius = value; }
        public string Description { get => description; set => description = value; }
        public int Type { get => type; set => type = value; }

        public Building(GameTextures texturecollection, GameWorld gameWorld, Point coordinates,  int effectradius, string description, int cost,bool isterminus, int type)
        {
            this.texture = texturecollection.GetTextures()["station"];
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            this.effectradius = effectradius;
            this.description = description;
            this.isterminus = isterminus;
            this.coordinates = coordinates;
            this.type = type;
            highlighttile = new Tile(0, texturecollection.GetTextures()["highlight_lightblue"], tileposition, new Rectangle(tileposition.ToPoint(),new Point(200,100)));

        }

        public string GetStationType()
        {
            switch (type)
            {
                case 1: return "Bus Stop";
                case 2: return "Trolley Stop";
                case 3: return "Tram Stop";
                case 4: return "Metro Stop";
                default: return "Undefined";
            }
            
        }

        public void CheckIfHighlighted(Point coordinatetocheck)
        {
            if (coordinatetocheck==coordinates)
            {
                highlighted = true;
            }
            else
            {
                highlighted = false;
            }
        }

        public void CalculateHighlightTile()
        {
        }

        private Vector2 DisplayPosition()
        {
            return new Vector2((tileposition.X + 150) - (texture.Width), (tileposition.Y + 75) - (texture.Height));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (highlighted)
            {
                for (int y = -effectradius; y < effectradius+1; y++)
                {
                    for (int x = -effectradius; x < effectradius+1; x++)
                    {
                        Vector2 arealocationisntance = new Vector2(highlighttile.Position.X+y*100-x*100, (highlighttile.Position.Y + y *50+x*50));
                        spriteBatch.Draw(highlighttile.Texture, arealocationisntance, Color.White);
                    }
                }
            }
            if (visible)
            {
                spriteBatch.Draw(texture, DisplayPosition(), Color.White);
            }
        }
    }
}