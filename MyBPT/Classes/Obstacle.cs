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
{     /// <summary>
      /// Blokádelem. A játéktéren jelenik meg, és a játékosnak el kell takarítania mielött az általa lefoglalt pocícióra épithet.
      /// </summary>
    class Obstacle
    {
        //Változók
        Texture2D texture;
        Vector2 tileposition;
        Point coordinates;
        bool highlighted;
        int cost;
        Button demolishbutton;
        Tile highlighttile;

        //Tulajdonságok
        public Button DemolishButton { get => demolishbutton; set => demolishbutton = value; }
        public Point Coordinates { get => coordinates; set => coordinates = value; }
        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public int Cost { get => cost; set => cost = value; }

        /// <summary>
        /// Blokád konstruktor: létrehoz egy megjeleníthető blokádot, majd létrehozza a HUD elemeket és inicializálja a megjelenítést.
        /// </summary>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="textureid">A blokádelem textúrájának azonosítója a textúragyüjteményen belül</param>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="coordinates">Ahol az épület található</param>
        ///  <param name="cost">A pénzmennyiség, amelyet a blokát eltakarításáért fizet a játékos</param>
        public Obstacle(Dictionary<string,Texture2D> texturecollection, string textureid, Point preferredscreensize, GameWorld gameWorld,Point coordinates,int cost)
        {

            this.texture = texturecollection[textureid];
            this.cost = cost;
            this.coordinates = coordinates;
            demolishbutton = new Button(new Vector2(0, 0), texturecollection["hud_button_demolish"]);
            demolishbutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 - demolishbutton.Texture.Width / 2, preferredscreensize.Y - 300));
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            ButtonVisibility_NoSelection();
        }


        /// <summary>
        /// A blokád játéktérre történő megrajzolása
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
        /// Az blokádhoz tartozó gombok HUD-ra való rajzolása. Csak akkor jelenik meg, ha azon elemek láthatósága: igaz
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