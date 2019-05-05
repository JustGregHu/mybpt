using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes {
    /// <summary>
    /// Lenyomható és megjeleníthető gomb.
    /// </summary>
    public class Button {

        //Változók
        Vector2 position;
        Rectangle area;
        bool visible;
        Texture2D texture;

        //Tulajdonságok
        public bool Visible { get => visible; set => visible = value; }
        public Vector2 Position { get => position; set => position = value; }
        public Texture2D Texture { get => texture; set => texture = value; }

        /// <summary>
        /// Gomb létrehozása. Hozzárendel a gombhoz egy texturát és egy pozíciót, majd érintést érzékelő felületet (hitoboxot) készít a textúra mérete és a megadott pozíció alapján.
        /// </summary>
        /// <param name="position">Gomb elhelyezkedése a képernyőn. A pozíciója megadja annak bal felső sarkát. </param>
        ///  <param name="texture">A gombon megjelenő textúra. (nélküle láthatalan lenne) </param>
        public Button(Vector2 position, Texture2D texture) {
            Visible = true;
            this.texture = texture;
            this.position = position;
            area = new Rectangle(position.ToPoint(),new Point(texture.Width, texture.Height));
        }

        /// <summary>
        /// Az gomb játéktérre való rajzolása
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                spriteBatch.Draw(texture, new Vector2(position.X, position.Y), Color.White);
            }
        }

        /// <summary>
        /// A gomb elhelyezkedésének frissítése. Ez újragenerálja a hitboxot is.
        /// </summary>
        /// <param name="position">Gomb elhelyezkedése a képernyőn. A pozíciója megadja annak bal felső sarkát. </param>
        public void UpdatePosition(Vector2 position)
        {
            this.position = position;
            area = new Rectangle(position.ToPoint(), new Point(texture.Width, texture.Height));
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott érintés pozíciója megtalálható a gomb hitboxán belül és ha az érintés a megadás pillanatában nincs letartot állapotban.
        /// </summary>
        /// <param name="tl">MonoGame érintés pozíciója</param>
        public bool IsTapped(TouchLocation tl) {
            if (tl.State == TouchLocationState.Pressed && this.area.Contains(tl.Position)) {
                return true;

            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott érintés pozíciója megtalálható a gomb hitboxán belül és ha az érintés letartva van amikor megadásra kerül.
        /// </summary>
        /// <param name="tl">MonoGame érintés pozíciója</param>
        public bool IsHeld(TouchLocation tl) {
            if (tl.State == TouchLocationState.Moved && this.area.Contains(tl.Position)) {
                return true;
            }
            return false;
        }
    }
}