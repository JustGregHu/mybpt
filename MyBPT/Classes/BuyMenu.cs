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
    class BuyMenu
    {
        bool buymenuisopen;
        bool selectingbuylocation;
        Button openbutton;
        Button closebutton;
        List<Button> choices = new List<Button>();
        Texture2D background;

        public Button OpenButton { get => openbutton; set => openbutton = value; }
        public Button CloseButton { get => closebutton; set => closebutton = value; }
        public bool BuymenuIsOpen { get => buymenuisopen; set => buymenuisopen = value; }
        public bool SelectingBuyLocation { get => selectingbuylocation; set => selectingbuylocation = value; }

        public BuyMenu(GraphicsDevice graphicsdevice, Point preferrecscreensize, GameTextures texturecollection)
        {
            Color[] data = new Color[preferrecscreensize.X * preferrecscreensize.Y];
            background = new Texture2D(graphicsdevice, preferrecscreensize.X, preferrecscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);

            choices.Add(new Button(new Vector2(400, 300), texturecollection.GetTextures()["buymenu_station"]));
            choices.Add(new Button(new Vector2(800, 300), texturecollection.GetTextures()["buymenu_terminus"]));
            openbutton = new Button(new Vector2(1000, 400), texturecollection.GetTextures()["buymenu_open"]);
            closebutton = new Button(new Vector2(preferrecscreensize.X-200, 100), texturecollection.GetTextures()["buymenu_close"]);
            CloseBuyMenu();
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (buymenuisopen)
            {
                spriteBatch.Draw(background, new Vector2(300,0), new Color(30, 30, 30,90));
                closebutton.Draw(spriteBatch);
                DrawChoices(spriteBatch);


            }
            else if(!selectingbuylocation)
            {
                openbutton.Draw(spriteBatch);
            }
        }

        private void ToggleChoicesVisibility(bool visibility)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                choices[i].Visible = visibility;
            }
        }

        private void DrawChoices(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                choices[i].Draw(spriteBatch);
            }
        }

        public void OpenBuyMenu()
        {
            ToggleChoicesVisibility(true);
            openbutton.Visible = false;
            closebutton.Visible = true;
            selectingbuylocation = false;
            buymenuisopen = true;
        }

        public void CloseBuyMenu()
        {
            ToggleChoicesVisibility(false);
            openbutton.Visible = true;
            closebutton.Visible = false;
            selectingbuylocation = false;
            buymenuisopen = false;
        }

    }
}