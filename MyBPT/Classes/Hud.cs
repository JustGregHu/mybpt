using Microsoft.Xna.Framework;
using com.bitbull.meat;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes {
    /// <summary>
    /// A gombok és játékinfó erre a nézetbeli rétegre kerülnek.
    /// </summary>
    class Hud {
        public Matrix transform;
        Viewport view;
        Vector2 position;
        Lerper lerper = new Lerper();

        /// <summary>
        /// Létrehozza a nézetet, majd inicializálja azt.
        /// </summary>
        /// <param name="newView">Ehhez a nézethez rendeljük a kamerát</param>
        public Hud (Viewport newView) {
            position = new Vector2(0, 0);
            view = newView;
        }

        /// <summary>
        /// Frissíti és a képernyőmérethez igazítja a nézetet.
        /// </summary>
        /// <param name="gameTime">A játékbeli eltelt idővel lépést tartó objektum.</param>
        /// <param name="tc">Monogame-hez tartozó érintésgyüjtemény</param>
        public void Update(GameTime gameTime, TouchCollection tc) {
            transform = Matrix.CreateTranslation(new Vector3(-position.X - view.Width / 2, -position.Y - view.Height / 2, 0));
        }
    }
}