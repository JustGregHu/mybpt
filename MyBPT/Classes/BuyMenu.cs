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
        Button stationbuybutton;
        Button terminusbuybutton;
        Texture2D background;

        public Button OpenButton { get => openbutton; set => openbutton = value; }
        public Button CloseButton { get => closebutton; set => closebutton = value; }
        public bool BuymenuIsOpen { get => buymenuisopen; set => buymenuisopen = value; }
        public bool SelectingBuyLocation { get => selectingbuylocation; set => selectingbuylocation = value; }
        internal Button TerminusBuyButton { get => terminusbuybutton; set => terminusbuybutton = value; }
        internal Button StationBuyButton { get => stationbuybutton; set => stationbuybutton = value; }

        public BuyMenu(GraphicsDevice graphicsdevice, Point preferredscreensize, GameTextures texturecollection)
        {
            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);

            stationbuybutton=(new Button(new Vector2(400, 300), texturecollection.GetTextures()["hud_button_buy_station"]));
            terminusbuybutton = (new Button(new Vector2(800, 300), texturecollection.GetTextures()["hud_button_buy_terminus"]));
            openbutton = new Button(new Vector2(1000, 400), texturecollection.GetTextures()["hud_button_buy_open"]);
            closebutton = new Button(new Vector2(preferredscreensize.X-200, 100), texturecollection.GetTextures()["hud_button_buy_close"]);
            CloseBuyMenu();
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (buymenuisopen)
            {
                spriteBatch.Draw(background, new Vector2(300,0), new Color(0, 0, 0, 85));
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
            stationbuybutton.Visible = visibility;
            TerminusBuyButton.Visible = visibility;
        }

        private void DrawChoices(SpriteBatch spriteBatch)
        {
            stationbuybutton.Draw(spriteBatch);
            TerminusBuyButton.Draw(spriteBatch);
        }

        public void OpenBuyMenu()
        {
            ToggleChoicesVisibility(true);
            openbutton.Visible = false;
            closebutton.Visible = true;
            stationbuybutton.Visible = true;
            stationbuybutton.Visible = true;
            selectingbuylocation = false;
            buymenuisopen = true;
        }

        public void CloseBuyMenu()
        {
            ToggleChoicesVisibility(false);
            openbutton.Visible = true;
            closebutton.Visible = false;
            selectingbuylocation = false;
            stationbuybutton.Visible = false;
            stationbuybutton.Visible = false;
            buymenuisopen = false;
        }

    }
}