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
    /// <summary>
    /// Egy "csempényi" útelemnek megfelelő objektum.
    /// </summary>
    public class Road
    {
        Texture2D texture;
        Vector2 tileposition;
        Point coordinates;

        public Point Coordinates { get => coordinates; set => coordinates = value; }

        /// <summary>
        /// Épület konstruktor: létrehoz egy megjeleníthető épületet. A megadott szint és típus által beállítja a szintent és az épület típusát. Létrehozza a HUD elemeket és inicializálja a megjelenítést.
        /// </summary>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        /// <param name="textureid">A blokádelem textúrájának azonosítója a textúragyüjteményen belül</param>
        /// <param name="coordinates">Ahol az épület található</param>
        public Road(Dictionary<string, Texture2D> texturecollection, string textureid, GameWorld gameWorld,Point coordinates)
        {
            this.texture = texturecollection[textureid];
            this.coordinates = coordinates;
            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
        }

        /// <summary>
        /// Az útelem a játéktérre történő rajzolása
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, tileposition, Color.White);
        }
    }
}