using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyBPT.Classes
{
    /// <summary>
    /// Befolyással rendelkező, megjeleníthető épületpobjektum. A játéktéren belül a játékos az épületek befolyását gyűjti az állomások hatásköre segítségével.
    /// </summary>
    class Building
    {
        //Változók
        int type;
        int level;
        int influenceamount;
        bool highlighted;
        Texture2D texture;
        Vector2 tileposition;
        Point coordinates;
        Button demolishbutton;
        Tile highlighttile;

        //Tulajdonságok
        public Button DemolishButton { get => demolishbutton; set => demolishbutton = value; }
        public Point Coordinates { get => coordinates; set => coordinates = value; }
        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public int Influenceamount { get => influenceamount; set => influenceamount = value; }

        /// <summary>
        /// Épület konstruktor: létrehoz egy megjeleníthető épületet. A megadott szint és típus által beállítja a szintent és az épület típusát. Létrehozza a HUD elemeket és inicializálja a megjelenítést.
        /// </summary>
        /// <param name="gametextures">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="coordinates">Ahol az épület található</param>
        /// <param name="type">Épülettípus. 0: polgári, 1: kereskedelmi, 2: ipari</param>
        ///  <param name="level">Bónuszokkál jár a magasabb szint. Jelenleg támogatott maximum: 2</param>
        public Building(GameTextures gametextures, GameWorld gameWorld, Point preferredscreensize, Point coordinates, int type, int level)
        {
            this.level = level;
            this.type = type;
            this.texture = gametextures.FindBuildingTexture(level, type);
            switch (type)
            {
                case 0:
                    SetInfluenceAmount(50*level);
                    break;
                case 1: SetInfluenceAmount(100* level);
                    break;
                case 2: SetInfluenceAmount(125* level);
                    break;
                default:
                    break;
            }
            this.coordinates = coordinates;
            demolishbutton = new Button(new Vector2(0,0),gametextures.GetTextures()["hud_button_demolish"]);
            demolishbutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 - demolishbutton.Texture.Width / 2, preferredscreensize.Y - 300));
            highlighttile = new Tile(0, gametextures.GetTextures()["world_highlight_lightblue"], tileposition, new Rectangle(tileposition.ToPoint(), new Point(200, 100)));
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            ButtonVisibility_NoSelection();
        }

        /// <summary>
        /// Az épület játéktérre való rajzolása
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
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

        /// <summary>
        /// Az épülethez tartozó gombok HUD-ra való rajzolása. Csak akkor jelenik meg, ha azon elemek láthatósága: igaz
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void DrawButtons(SpriteBatch spriteBatchHud)
        {
            DemolishButton.Draw(spriteBatchHud);
        }

        /// <summary>
        /// Frissíti azt, hogy a kurzor a jelenlegi objektumon tartózkodik-e.
        /// </summary>
        /// <param name="coordinatetocheck">Ellenőrizendő koordináta</param>
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

        /// <summary>
        /// Az épület befolyását frissíti a megadott értékkel.
        /// </summary>
        /// <param name="cost">Befolyás (pénz érték)</param>
        public void SetInfluenceAmount(int cost)
        {
            influenceamount = cost+ level*(cost * type);
        }

        /// <summary>
        /// Típus száma alapján stringként adja vissza az épület típusát.
        /// </summary>
        /// <param name="whattype">Típus száma (0-2)</param>
        public string GetTypeString(int whattype)
        {
            switch (whattype)
            {
                case 0: return "Residential";
                case 1: return "Commercial";
                case 2: return "Industrial";
                default: return "Undefined";
            }
        }

        /// <summary>
        /// Az épületobjektumot a pozíciója alapján az alatta lévő "csempére" igazítja
        /// </summary>
        private Vector2 DisplayPosition()
        {
            return new Vector2((tileposition.X + 200) - (texture.Width), (tileposition.Y + 150) - (texture.Height));
        }

        //Láthatóságot irányító funkciók
        private void ButtonVisibility_NoSelection()
        {
            DemolishButton.Visible = false;
        }
        private void ButtonVisibility_Highlighted()
        {
            DemolishButton.Visible = true;
        }


    }
}