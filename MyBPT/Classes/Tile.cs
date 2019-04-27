using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyBPT.Classes {
    /// <summary>
    /// Egy "csempe" objektum. Csempékből epül fel a játékban található, négyzetekből összerakott terep. Izometrikus pozíciót kapnak.
    /// </summary>
    class Tile {
        //Változók
        int height;
        Texture2D texture;
        Vector2 position;
        Vector2 tempposition;
        Rectangle area;
        bool moving;
        bool highlighted;

        //Tulajdonságok
        public Texture2D Texture { get => texture; set => texture = value; }
        public Rectangle Area { get => area; set => area = value; }
        public Vector2 Position { get => position; set => position = value; }
        public bool Moving { get => moving; set => moving = value; }
        public int Height { get => height; set => height = value; }
        public bool Highlighted { get => highlighted; set => highlighted = value; }

        /// <summary>
        /// Létrehoz egy új csempét, átadja a megadott változókat és inicializálja a megjelenést.
        /// </summary>
        /// <param name="area">A csempe hitboxa</param>
        /// <param name="height">A csempén található terep magassága</param>
        /// <param name="texture">A csempéhez tartozó textúra</param>
        /// <param name="position">A csempe Izometrikus pozíciója</param>
        public Tile(int height, Texture2D texture, Vector2 position, Rectangle area) {
            this.height = height;
            this.texture = texture;
            this.position = position;
            this.tempposition = position;
            this.Area = area;
            this.moving = false;
            highlighted = false;
        }

        /// <summary>
        /// A csempe játéktérre történő megrajzolása. Azesetben, ha a csempén van a kurzor, a kiemelő textúra is megjelenik
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void Draw(SpriteBatch spriteBatch) {
            if (highlighted)
            {
                spriteBatch.Draw(new Texture2D(spriteBatch.GraphicsDevice, 100, 100), position, Color.White);
            }
            else
            {
                spriteBatch.Draw(texture, position, Color.White);
            }
        }
    }
}