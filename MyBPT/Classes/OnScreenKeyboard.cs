using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes
{
    /// <summary>
    /// Képernyőbillentyűzet objektum. Segítségével bekérhetünk a játékostól egy sor szöveget (stringet)
    /// </summary>
    public class OnScreenKeyboard
    {
        //Változók
        string currenttext;
        Button deletebutton;
        Button submitbutton;
        List<KeyboardKeys> keys = new List<KeyboardKeys>();
        Texture2D background;
        bool isopen;
        char[] keyboardkeys = new char[] { 'q','w','e','r','t','z','u','i','o','p','a','s','d','f','g','h','j','k','l','y','x','c','v','b','n','m'};
        
        //Tulajdonságok
        public string CurrentText { get => currenttext; set => currenttext = value; }
        internal Button SubmitButton { get => submitbutton; set => submitbutton = value; }
        public bool KeyboardIsOpen { get => isopen; set => isopen = value; }

        /// <summary>
        /// Létrehozza a billenyűzet objektumot. Inicializálja a működéshez szükséges elemeket, létrehozza a gombokat és elhelyezi azokat.
        /// </summary>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="graphicsdevice">MonoGame-hez tartozó grafikai készlet</param>
        public OnScreenKeyboard(GraphicsDevice graphicsdevice, Point preferredscreensize,GameTextures texturecollection)
        {
            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);

            currenttext = "";
            deletebutton = new Button(new Vector2(950,400), texturecollection.GetTextures()["hud_keyboard_key_del"]);
            submitbutton = new Button(new Vector2(950, 500), texturecollection.GetTextures()["hud_keyboard_submit"]);
            int horizontaloffset = 250;
            int verticaloffset = 0;
            int currentrowcount = 0;
            for (int i = 0; i < keyboardkeys.Length; i++)
            {
                if (i==10)
                {
                    horizontaloffset = 270;
                    verticaloffset = 55;
                    currentrowcount = 0;
                }
                if (i==19)
                {
                    horizontaloffset = 290;
                    verticaloffset = 55 * 2;
                    currentrowcount = 0;
                }
                keys.Add(new KeyboardKeys(new Button(new Vector2(currentrowcount * 54+ horizontaloffset, 400+ verticaloffset), texturecollection.GetTextures()["hud_keyboard_key"]), keyboardkeys[i]));
                currentrowcount++;
            }
            CloseKeyboard();
        }


        /// <summary>
        /// A billenyűzethet tartozó gombok és elemek rajzolása. Csak akkor jelenik meg, ha azon elemek láthatósága: igaz
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        /// <param name="font">MonoGame betűgyüjtemény, amelyet beállításai alapján rajzolódik a funckióban megadott szöveg</param>
        public void DrawOnScreenKeyboard(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (isopen)
            {
                spriteBatch.Draw(background, new Vector2(0, 0), new Color(0, 0, 0, 90));
                for (int i = 0; i < keys.Count; i++)
                {
                    keys[i].Button.Draw(spriteBatch);
                    spriteBatch.DrawString(font, keys[i].Key.ToString(), new Vector2(keys[i].Button.Position.X + 5, keys[i].Button.Position.Y), Color.Black);
                }
                submitbutton.Draw(spriteBatch);
                deletebutton.Draw(spriteBatch);
                spriteBatch.DrawString(font, currenttext, new Vector2(500, 350), Color.White);
            }
        }

        /// <summary>
        /// Hozzáad a beolvasott string-hez egy lenyomott billentyűgombnak megfelelő karaktert
        /// </summary>
        /// <param name="tc">Monogame-hez tartozó érintésgyüjtemény</param>
        public void AddPressedKey(TouchCollection tc)
        {
            if (tc.Count>0)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i].Button.IsTapped(tc[0]) && keys[i].Button.Visible)
                    {
                        currenttext+=keys[i].Key;
                    }
                }
                try
                {
                    if (deletebutton.IsTapped(tc[0]) && deletebutton.Visible)
                    {
                        DeleteLastKey(tc);
                    }
                }
                catch (Exception){}
            }
        }

        /// <summary>
        /// Eltörli a beolvasott string utolsó karakterét, ha a string nem üres
        /// </summary>
        /// <param name="tc">Monogame-hez tartozó érintésgyüjtemény</param>
        public void DeleteLastKey(TouchCollection tc)
        {
            if (tc.Count > 0)
            {
                if (currenttext.Length>0 && deletebutton.IsTapped(tc[0]) && deletebutton.Visible)
                {
                    currenttext = currenttext.Remove(currenttext.Length - 1);
                }
            }
        }

        //Láthatóságot kezelő funkciók
        public void OpenKeyboard()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                keys[i].Button.Visible = true;
            }
            submitbutton.Visible = true;
            deletebutton.Visible = true;
            isopen = true;
        }
        public void CloseKeyboard()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                keys[i].Button.Visible = false;
            }
            submitbutton.Visible = false;
            deletebutton.Visible = false;
            isopen = false;
            CurrentText = "";
        }
    }
}