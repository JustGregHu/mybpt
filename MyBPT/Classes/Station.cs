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
    class Station {
        Texture2D texture;
        Vector2 tileposition;
        bool highlighted;
        bool moving;
        int effectradius;
        String description;
        int cost;
        int sellprice;
        bool isterminus;
        bool visible = true;
        Tile highlighttile;
        int type;

        Point coordinates;
        Point safecoordinates;
        Vector2 safeposition;
        Button movebutton;
        Button sellbutton;
        Button upgradebutton;
        Button acceptbutton;
        Button cancelbutton;

        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public bool Isterminus { get => isterminus; set => isterminus = value; }
        public int Effectradius { get => effectradius; set => effectradius = value; }
        public string Description { get => description; set => description = value; }
        public int Type { get => type; set => type = value; }
        public Button MoveButton { get => movebutton; set => movebutton = value; }
        public Button SellButton { get => sellbutton; set => sellbutton = value; }
        public Button UpgradeButton { get => upgradebutton; set => upgradebutton = value; }
        public Button AcceptButton { get => acceptbutton; set => acceptbutton = value; }
        public Button CancelButton { get => cancelbutton; set => cancelbutton = value; }
        public Point Coordinates { get => coordinates; set => coordinates = value; }
        public bool Moving { get => moving; set => moving = value; }

        public Station(GameTextures texturecollection, GameWorld gameWorld, Point coordinates, int effectradius, string description, int cost, bool isterminus, int type)
        {
            movebutton = new Button(new Vector2(400, 50), texturecollection.GetTextures()["buildingmenu_move"]);
            sellbutton = new Button(new Vector2(550, 50), texturecollection.GetTextures()["buildingmenu_sell"]);
            upgradebutton = new Button(new Vector2(700, 50), texturecollection.GetTextures()["buildingmenu_upgrade"]);
            acceptbutton = new Button(new Vector2(500, 200), texturecollection.GetTextures()["selection_tick"]);
            cancelbutton = new Button(new Vector2(650, 200), texturecollection.GetTextures()["selection_cross"]);

            if (isterminus)
            {
                this.texture = texturecollection.GetTextures()["terminus"];
            } else
            {
                this.texture = texturecollection.GetTextures()["station"];
            }
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            this.effectradius = effectradius;
            this.description = description;
            this.isterminus = isterminus;
            this.cost = cost;
            this.coordinates = coordinates;
            this.type = type;
            highlighttile = new Tile(0, texturecollection.GetTextures()["highlight_lightblue"], tileposition, new Rectangle(tileposition.ToPoint(), new Point(200, 100)));
            ButtonVisibility_NoSelection();
        }

        public int SellPrice
        {
            get
            {
                return cost / 2;
            }
        }

        public int Cost { get => cost; set => cost = value; }

        private void ButtonVisibility_NoSelection()
        {
            movebutton.Visible = false;
            sellbutton.Visible = false;
            upgradebutton.Visible = false;
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;

        }
        private void ButtonVisibility_Highlighted()
        {
            movebutton.Visible = true;
            sellbutton.Visible = true;
            upgradebutton.Visible = true;
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;
        }
        private void ButtonVisibility_Moving()
        {
            movebutton.Visible = false;
            sellbutton.Visible = false;
            upgradebutton.Visible = false;
            acceptbutton.Visible = true;
            cancelbutton.Visible = true;
        }

        public void InitiateMove()
        {
            moving = true;
            safeposition = tileposition;
            safecoordinates = coordinates;
            ButtonVisibility_Moving();
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;
        }

        public void FinalizeMove(GameWorld gameworld)
        {
            moving = false;
            safeposition = tileposition;
            safecoordinates = coordinates;
            highlighttile.Position = tileposition;
            ButtonVisibility_NoSelection();
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;
        }

        public void MoveStationTo(Point newcoordinates,GameWorld gameworld)
        {
            if (newcoordinates.X<0)
            {
                newcoordinates = new Point(0, newcoordinates.Y);
            }
            if (newcoordinates.Y < 0)
            {
                newcoordinates = new Point(newcoordinates.X, 0);
            }
            this.tileposition = gameworld.MapData[newcoordinates.X, newcoordinates.Y].Position;
            highlighttile.Position = tileposition;
            coordinates = newcoordinates;
            ButtonVisibility_Moving();
        }

        public void RollbackMove()
        {
            moving = false;
            coordinates = safecoordinates;
            tileposition = safeposition;
            highlighttile.Position = tileposition;
            ButtonVisibility_NoSelection();
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

        private Vector2 DisplayPosition()
        {
            return new Vector2((tileposition.X + 150) - (texture.Width), (tileposition.Y + 75) - (texture.Height));
        }

        public void DrawAffectionOfArea(SpriteBatch spriteBatch)
        {
            if (highlighted)
                for (int y = -effectradius; y < effectradius + 1; y++)
                    {
                        for (int x = -effectradius; x < effectradius + 1; x++)
                        {
                            Vector2 arealocationisntance = new Vector2(highlighttile.Position.X + y * 100 - x * 100, (highlighttile.Position.Y + y * 50 + x * 50));
                            spriteBatch.Draw(highlighttile.Texture, arealocationisntance, Color.White);
                        }
                    }
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (highlighted)
            {
                if (moving)
                {
                    ButtonVisibility_Moving();
                }
                else
                {
                    ButtonVisibility_Highlighted();
                }
            }
            if (visible)
            {
                spriteBatch.Draw(texture, DisplayPosition(), Color.White);
            }
        }

        public void DrawButtons(SpriteBatch spriteBatchHud)
        {
            if (moving)
            {
                AcceptButton.Draw(spriteBatchHud);
                CancelButton.Draw(spriteBatchHud);
            }
            else
            {
                MoveButton.Draw(spriteBatchHud);
                SellButton.Draw(spriteBatchHud);
                UpgradeButton.Draw(spriteBatchHud);
            }

        }
    }
}