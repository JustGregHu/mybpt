using com.bitbull.meat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes
{
    /// <summary>
    /// Mozgatható kamera. Lehetővé teszi a nézet mozgatását a játéktéren belül.
    /// </summary>
    class Camera
    {
        //Változók
        public Matrix transform;
        Viewport view;
        Vector2 position;
        Vector2 targetposition;
        float zoomamount;
        Lerper lerper = new Lerper();

        //Tulajdonságok
        public Vector2 TargetPosition { get => targetposition; set => targetposition = value; }
        public Vector2 Position { get => position; set => position = value; }

        /// <summary>
        /// Létrehozza a kamerát, majd inicializálja a nézetet.
        /// </summary>
        /// <param name="newView">Ehhez a nézethez rendeljük a kamerát</param>
        public Camera(Viewport newView)
        {
            targetposition = new Vector2(0, 0);
            position = new Vector2(0, 0);
            view = newView;
            zoomamount = 1;
        }

        /// <summary>
        /// Frissíti a kamera elhelyezkedését. A megadott gametime segítségével lépést tart a játékidővel és az érintésgyüjteményből kinyert érintés helyzet + pozíció alapján mozgatja a kamerát. Mozgását a lerper simítja.
        /// </summary>
        /// <param name="gameTime">A játékbeli eltelt idővel lépést tartó objektum.</param>
        /// <param name="tc">Monogame-hez tartozó érintésgyüjtemény</param>
        public void Update(GameTime gameTime,TouchCollection tc)
        {
            if (tc.Count>0)
            {
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (GestureType.FreeDrag == gs.GestureType)
                    {
                        targetposition = new Vector2(targetposition.X - gs.Delta.X, targetposition.Y - gs.Delta.Y);
                    }
                }
            }
            position.X= lerper.Lerp(position.X, targetposition.X);
            position.Y = lerper.Lerp(position.Y, targetposition.Y);
            transform = Matrix.CreateTranslation(new Vector3(-position.X-view.Width / 2, -position.Y-view.Height / 2, 0))*Matrix.CreateScale(new Vector3(Zoom,Zoom,0))*Matrix.CreateTranslation(new Vector3(view.Width / 2, view.Height / 2, 0));
        }

        /// <summary>
        /// Visszatér a kamera nagyítási értékével, illetve rajta keresztül az meg is adható.
        /// </summary>
        public float Zoom
        {
            get
            {
                return zoomamount;
            }
            set
            {
                zoomamount = value;
            }
        }
    }
}