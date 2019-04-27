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
        int backgroundstartingpositionX;
        Texture2D gameplaystats_coins;
        Point preferredscreensize;

        public Button OpenButton { get => openbutton; set => openbutton = value; }
        public Button CloseButton { get => closebutton; set => closebutton = value; }
        public bool BuymenuIsOpen { get => buymenuisopen; set => buymenuisopen = value; }
        public bool SelectingBuyLocation { get => selectingbuylocation; set => selectingbuylocation = value; }
        internal Button TerminusBuyButton { get => terminusbuybutton; set => terminusbuybutton = value; }
        internal Button StationBuyButton { get => stationbuybutton; set => stationbuybutton = value; }

        public BuyMenu(GraphicsDevice graphicsdevice, Point preferredscreensize, GameTextures texturecollection,int hudmargin)
        {
            Vector2 initialposition = new Vector2(0, 0);
            this.preferredscreensize = preferredscreensize;
            stationbuybutton =(new Button(initialposition, texturecollection.GetTextures()["hud_button_buy_station"]));
            terminusbuybutton = (new Button(initialposition, texturecollection.GetTextures()["hud_button_buy_terminus"]));
            openbutton = new Button(initialposition, texturecollection.GetTextures()["hud_button_buy_open"]);
            closebutton = new Button(initialposition, texturecollection.GetTextures()["hud_menu_close"]);
            gameplaystats_coins= texturecollection.GetTextures()["hud_gameplaystats_coins"];

            stationbuybutton.UpdatePosition(new Vector2(preferredscreensize.X-terminusbuybutton.Texture.Width-hudmargin*2-stationbuybutton.Texture.Width,preferredscreensize.Y/2-stationbuybutton.Texture.Height/2));
            terminusbuybutton.UpdatePosition(new Vector2(preferredscreensize.X - terminusbuybutton.Texture.Width - hudmargin, preferredscreensize.Y / 2 - stationbuybutton.Texture.Height / 2));
            openbutton.UpdatePosition(new Vector2(preferredscreensize.X - hudmargin - openbutton.Texture.Width, preferredscreensize.Y - hudmargin - openbutton.Texture.Height));
            closebutton.UpdatePosition(new Vector2(preferredscreensize.X - hudmargin - openbutton.Texture.Width, preferredscreensize.Y - hudmargin - openbutton.Texture.Height));

            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);

            backgroundstartingpositionX =preferredscreensize.X - terminusbuybutton.Texture.Width - hudmargin * 3 - stationbuybutton.Texture.Width;

            CloseBuyMenu();
        }

        public void Draw(SpriteBatch spriteBatch,GameWorld gameWorld,SpriteFont font)
        {
            if (buymenuisopen)
            {
                spriteBatch.Draw(background, new Vector2(backgroundstartingpositionX, 0), new Color(0, 0, 0, 85));
                closebutton.Draw(spriteBatch);
                DrawChoices(spriteBatch);
                DrawInfo(spriteBatch, gameWorld, font);
            }
            else if(!selectingbuylocation)
            {
                openbutton.Draw(spriteBatch);
            }
        }

        public void DrawInfo(SpriteBatch spriteBatch,GameWorld gameWorld,SpriteFont font)
        {
            spriteBatch.Draw(gameplaystats_coins, new Vector2(preferredscreensize.X-600,45), Color.White);
            spriteBatch.DrawString(font,"Current income: "+gameWorld.CurrentIncome, new Vector2(preferredscreensize.X - 550, 40), Color.White);
            spriteBatch.DrawString(font, "Stations: " + gameWorld.CountStationsOnMap(), new Vector2(preferredscreensize.X - 550, 90), Color.White);
            spriteBatch.DrawString(font, "Termini: " + gameWorld.CountTerminiOnMap(), new Vector2(preferredscreensize.X - 550, 140), Color.White);
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