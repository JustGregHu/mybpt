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
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes
{
    class OnScreenKeyboard
    {
        string currenttext;
        Button deletebutton;
        Button submitbutton;
        List<KeyboardKeys> keys = new List<KeyboardKeys>();
        Texture2D background;
        bool isopen;
        char[] keyboardkeys = new char[] { 'q','w','e','r','t','z','u','i','o','p','a','s','d','f','g','h','j','k','l','y','x','c','v','b','n','m'};
        

        public string CurrentText { get => currenttext; set => currenttext = value; }
        internal Button SubmitButton { get => submitbutton; set => submitbutton = value; }
        public bool KeyboardIsOpen { get => isopen; set => isopen = value; }

        public OnScreenKeyboard(GraphicsDevice graphicsdevice, Point preferredscreensize,GameTextures texturecollection)
        {
            currenttext = "";
            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);
            deletebutton = new Button(new Vector2(1000,350), texturecollection.GetTextures()["hud_keyboard_key_del"]);
            submitbutton = new Button(new Vector2(500, 650), texturecollection.GetTextures()["hud_keyboard_submit"]);
            int horizontaloffset = 400;
            int verticaloffset = 0;
            int currentrowcount = 0;
            for (int i = 0; i < keyboardkeys.Length; i++)
            {
                if (i==10)
                {
                    horizontaloffset = 420;
                    verticaloffset = 55;
                    currentrowcount = 0;
                }
                if (i==20)
                {
                    horizontaloffset = 440;
                    verticaloffset = 55 * 2;
                    currentrowcount = 0;
                }
                keys.Add(new KeyboardKeys(new Button(new Vector2(currentrowcount * 54+ horizontaloffset, 300+ verticaloffset), texturecollection.GetTextures()["hud_keyboard_key"]), keyboardkeys[i]));
                currentrowcount++;
            }
            CloseKeyboard();
        }

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
                catch (Exception)
                {

                   
                }
            }
        }

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

        public void DrawOnScreenKeyboard(SpriteBatch spriteBatch,SpriteFont font)
        {
            if (isopen)
            {
                spriteBatch.Draw(background, new Vector2(0, 0), new Color(0, 0, 0, 90));
                for (int i = 0; i < keys.Count; i++)
                {
                    keys[i].Button.Draw(spriteBatch);
                    spriteBatch.DrawString(font, keys[i].Key.ToString(), new Vector2(keys[i].Button.Position.X+5, keys[i].Button.Position.Y), Color.Black);
                }
                submitbutton.Draw(spriteBatch);
                deletebutton.Draw(spriteBatch);
                spriteBatch.DrawString(font, currenttext, new Vector2(600, 200), Color.White);
            }
        }
    }

    class KeyboardKeys
    {
        Button button;
        char key;

        public KeyboardKeys(Button button, char key)
        {
            this.button = button;
            this.key = key;
        }
        public char Key { get => key; set => key = value; }
        internal Button Button { get => button; set => button = value; }
    }
}