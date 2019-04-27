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
    /// Főmenü objektum. Segítségével nagiválhat a játékos menüpontok és funkciók között, indíthat játékot stb.
    /// </summary>
    class Menu
    {
        //Változók
        bool isgamesessionactive;
        bool menuisopen;
        bool scoresarevisible;
        bool loadscreen;
        bool playmenuisopen;
        Point preferredscreensize;
        Button logo;
        Texture2D background;
        Button openbutton;
        Button closebutton;
        Button gotoplaymenu;
        Button gotooptions;
        Button gotoscores;
        Button gotoexit;
        Button gotomainmenu;
        Button playregular;
        Button playhard;
        Button playsandbox;
        Button togglesound;
        Button resetstats;
        Button loadscreenvisual;

        //Tulajdonságok
        public bool MenuIsOpen { get => menuisopen; set => menuisopen = value; }
        internal Button GoToPlaymenuButton { get => gotoplaymenu; set => gotoplaymenu = value; }
        internal Button GoToOptionsButton { get => gotooptions; set => gotooptions = value; }
        internal Button GoToScoresButton { get => gotoscores; set => gotoscores = value; }
        internal Button GoToExitButton { get => gotoexit; set => gotoexit = value; }
        internal Button GoToMainmenuButton { get => gotomainmenu; set => gotomainmenu = value; }
        internal Button PlayRegularButton { get => playregular; set => playregular = value; }
        internal Button PlayHardButton { get => playhard; set => playhard = value; }
        internal Button PlaySandboxButton { get => playsandbox; set => playsandbox = value; }
        internal Button ToggleSoundButton { get => togglesound; set => togglesound = value; }
        internal Button ResetStatsButton { get => resetstats; set => resetstats = value; }
        internal Button OpenButton { get => openbutton; set => openbutton = value; }
        internal Button CloseButton { get => closebutton; set => closebutton = value; }
        public bool ScoresAreVisible { get => scoresarevisible; set => scoresarevisible = value; }

        /// <summary>
        /// Tudatja a menüvel, hogy a játékmenet megkezdődött. Ezután láthatóvá válik a closebutton és az openbutton.
        /// </summary>
        public void StartGameSession()
        {
            isgamesessionactive = true;
        }

        /// <summary>
        /// Tudatja a menüvel, hogy a játékmenet véget ért. Ezután rejtetté válik a closebutton és az openbutton.
        /// </summary>
        public void EndGameSession()
        {
            isgamesessionactive = false;
        }

        /// <summary>
        /// Létrehozza a menü objektumot. Elmenti az ajánlott képernyőméretet, inicializálja a működéshez szükséges elemeket, létrehozza a gombokat és elhelyezi azokat.
        /// </summary>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="graphicsdevice">MonoGame-hez tartozó grafikai készlet</param>
        /// <param name="hudmargin">Pixelben megadott távolság a HUD elemek és a képernyő között</param>
        public Menu(GraphicsDevice graphicsdevice, Point preferredscreensize, GameTextures texturecollection, int hudmargin)
        {
            //inicializálás
            this.preferredscreensize = preferredscreensize;
            loadscreen = false;
            menuisopen = true;
            isgamesessionactive = false;

            //háttér
            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);

            //gombtextúrák felvétele
            Vector2 initposition = new Vector2(0, 0);
            logo = (new Button(initposition, texturecollection.GetTextures()["hud_menu_LOGO"]));
            openbutton = (new Button(initposition, texturecollection.GetTextures()["hud_menu_open"]));
            closebutton = (new Button(initposition, texturecollection.GetTextures()["hud_menu_close"]));
            gotoplaymenu = (new Button(initposition, texturecollection.GetTextures()["hud_menu_play"]));
            gotoscores = (new Button(initposition, texturecollection.GetTextures()["hud_menu_scores"]));
            gotooptions = (new Button(initposition, texturecollection.GetTextures()["hud_menu_options"]));
            gotoexit = (new Button(initposition, texturecollection.GetTextures()["hud_menu_exit"]));
            gotomainmenu = (new Button(initposition, texturecollection.GetTextures()["hud_menu_back"]));
            playregular = (new Button(initposition, texturecollection.GetTextures()["hud_menu_play_normal"]));
            playhard = (new Button(initposition, texturecollection.GetTextures()["hud_menu_play_hardmode"]));
            playsandbox = (new Button(initposition, texturecollection.GetTextures()["hud_menu_play_sandbox"]));
            togglesound = (new Button(initposition, texturecollection.GetTextures()["hud_menu_play_options_sound_on"]));
            resetstats = (new Button(initposition, texturecollection.GetTextures()["hud_menu_play_options_reset"]));
            loadscreenvisual = (new Button(initposition, texturecollection.GetTextures()["game_loading"]));

            //gombok elheyezése
            int cardmargin = 50;
            logo.UpdatePosition(new Vector2(preferredscreensize.X / 2 - logo.Texture.Width / 2, 50));
            openbutton.UpdatePosition(new Vector2(preferredscreensize.X - openbutton.Texture.Width - hudmargin, hudmargin));
            closebutton.UpdatePosition(new Vector2(preferredscreensize.X - openbutton.Texture.Width - hudmargin, hudmargin));
            gotoplaymenu.UpdatePosition(new Vector2(preferredscreensize.X / 2 - gotoplaymenu.Texture.Width / 2, preferredscreensize.Y/2-gotoplaymenu.Texture.Height / 2));
            gotoscores.UpdatePosition(new Vector2(preferredscreensize.X / 2 - gotoscores.Texture.Width / 2, preferredscreensize.Y / 2+gotoscores.Texture.Height/2));
            gotooptions.UpdatePosition(new Vector2(preferredscreensize.X - gotooptions.Texture.Width*2 - hudmargin*2, preferredscreensize.Y - gotooptions.Texture.Height - hudmargin));
            gotoexit.UpdatePosition(new Vector2(preferredscreensize.X - gotoexit.Texture.Width - hudmargin, preferredscreensize.Y - gotoexit.Texture.Height - hudmargin));
            gotomainmenu.UpdatePosition(new Vector2(preferredscreensize.X / 2 - gotomainmenu.Texture.Width / 2, preferredscreensize.Y - 150));
            playregular.UpdatePosition(new Vector2(preferredscreensize.X / 2 - cardmargin - playregular.Texture.Width - playregular.Texture.Width / 2, preferredscreensize.Y / 2 - playregular.Texture.Height / 2));
            playhard.UpdatePosition(new Vector2(preferredscreensize.X / 2 - playhard.Texture.Width / 2, preferredscreensize.Y / 2 - playhard.Texture.Height / 2));
            playsandbox.UpdatePosition(new Vector2(preferredscreensize.X / 2 + cardmargin + playsandbox.Texture.Width / 2, preferredscreensize.Y / 2 - playsandbox.Texture.Height / 2));
            togglesound.UpdatePosition(new Vector2(preferredscreensize.X / 2 - cardmargin - togglesound.Texture.Width, preferredscreensize.Y / 2 - togglesound.Texture.Height / 2));
            resetstats.UpdatePosition(new Vector2(preferredscreensize.X / 2 + cardmargin , preferredscreensize.Y / 2 - resetstats.Texture.Height / 2));
            loadscreenvisual.UpdatePosition(new Vector2(preferredscreensize.X / 2 - loadscreenvisual.Texture.Width / 2, preferredscreensize.Y / 2 - loadscreenvisual.Texture.Height / 2));

            //menü megnyitása
            OpenMainMenu();
        }

        /// <summary>
        /// Az menühöz tartozó gombok és elemek rajzolása. Csak akkor jelenik meg, ha azon elemek láthatósága: igaz
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        /// <param name="font">MonoGame betűgyüjtemény, amelyet beállításai alapján rajzolódik a funckióban megadott szöveg</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (isgamesessionactive&&!menuisopen)
            {
                openbutton.Draw(spriteBatch);
            }
            if (menuisopen)
            {
                if (isgamesessionactive)
                {
                    spriteBatch.Draw(background, new Vector2(0, 0), new Color(0, 0, 0, 90));
                    closebutton.Draw(spriteBatch);
                }
                else
                {
                    spriteBatch.Draw(background, new Vector2(0, 0), new Color(0, 114, 188));
                }
                logo.Draw(spriteBatch);
                gotoplaymenu.Draw(spriteBatch);
                gotooptions.Draw(spriteBatch);
                gotoscores.Draw(spriteBatch);
                gotoexit.Draw(spriteBatch);
                gotomainmenu.Draw(spriteBatch);
                playregular.Draw(spriteBatch);
                playhard.Draw(spriteBatch);
                playsandbox.Draw(spriteBatch);
                togglesound.Draw(spriteBatch);
                resetstats.Draw(spriteBatch);
            }
            if (loadscreen)
            {
                loadscreenvisual.Draw(spriteBatch);
            }
            if (playmenuisopen)
            {
                spriteBatch.DrawString(font, "Your objective is to amass as much income as possible", new Vector2(210,60), Color.White);
                spriteBatch.DrawString(font, "within two years of ingame time.", new Vector2(390, 105), Color.White);
            }
        }

        /// <summary>
        /// Megjeleníti a pontszámlistát, a megadott listából nyeri az adatokat
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        /// <param name="font">MonoGame betűgyüjtemény, amelyet beállításai alapján rajzolódik a funckióban megadott szöveg</param>
        /// <param name="scores">Előre elkészített pontszám szövegrészek gyüjteménye (soronként)</param>
        public void DrawScores(SpriteBatch spriteBatch,SpriteFont font, List<String> scores)
        {
            int max = 10;
            if (scores.Count<10)
            {
                max = scores.Count;
            }
            if (scoresarevisible)
            {
                for (int i = 0; i < max; i++)
                {
                    spriteBatch.DrawString(font, scores[i]+" cash of total income", new Vector2(50, 50 + 50 * i),Color.White);
                }
            }
        }

        //Megjelenítést kezelő funkciók
        public void OpenMainMenu()
        {
            menuisopen = true;
            loadscreen = false;
            playmenuisopen = false;
            scoresarevisible = false;
            gotoplaymenu.Visible = true;
            gotooptions.Visible = true;
            gotoscores.Visible = true;
            gotoexit.Visible = true;
            gotomainmenu.Visible = false;
            playregular.Visible = false;
            playhard.Visible = false;
            playsandbox.Visible = false;
            togglesound.Visible = false;
            resetstats.Visible = false;
            openbutton.Visible = false;
            closebutton.Visible = true;
            logo.Visible = true;
        }
        public void CloseMenu()
        {
            menuisopen = false;
            loadscreen = false;
            playmenuisopen = false;
            scoresarevisible = false;
            gotoplaymenu.Visible = false;
            gotooptions.Visible = false;
            gotoscores.Visible = false;
            gotoexit.Visible = false;
            gotomainmenu.Visible = false;
            playregular.Visible = false;
            playhard.Visible = false;
            playsandbox.Visible = false;
            togglesound.Visible = false;
            resetstats.Visible = false;
            openbutton.Visible = true;
            closebutton.Visible = false;
            logo.Visible = false;
        }   
        public void OpenPlayMenu()
        {
            playmenuisopen = true;
            menuisopen = true;
            loadscreen = false;
            scoresarevisible = false;
            gotoplaymenu.Visible = false;
            gotooptions.Visible = false;
            gotoscores.Visible = false;
            gotoexit.Visible = false;
            gotomainmenu.Visible = true;
            playregular.Visible = true;
            playhard.Visible = true;
            playsandbox.Visible = true;
            togglesound.Visible = false;
            resetstats.Visible = false;
            openbutton.Visible = false;
            closebutton.Visible = true;
            logo.Visible = false;
        }
        public void OpenOptions()
        {
            menuisopen = true;
            loadscreen = false;
            playmenuisopen = false;
            scoresarevisible = false;
            gotoplaymenu.Visible = false;
            gotooptions.Visible = false;
            gotoscores.Visible = false;
            gotoexit.Visible = false;
            gotomainmenu.Visible = true;
            playregular.Visible = false;
            playhard.Visible = false;
            playsandbox.Visible = false;
            togglesound.Visible = true;
            resetstats.Visible = true;
            openbutton.Visible = false;
            closebutton.Visible = true;
            logo.Visible = false;
        }
        public void OpenScores()
        {
            menuisopen = true;
            loadscreen = false;
            playmenuisopen = false;
            scoresarevisible = true;
            gotoplaymenu.Visible = false;
            gotooptions.Visible = false;
            gotoscores.Visible = false;
            gotoexit.Visible = false;
            gotomainmenu.Visible = true;
            playregular.Visible = false;
            playhard.Visible = false;
            playsandbox.Visible = false;
            togglesound.Visible = false;
            resetstats.Visible = false;
            openbutton.Visible = false;
            closebutton.Visible = true;
            logo.Visible = false;
        }
        public void ShowLoadScreen()
        {
            menuisopen = true;
            loadscreen = true;
            scoresarevisible = false;
            gotoplaymenu.Visible = false;
            gotooptions.Visible = false;
            gotoscores.Visible = false;
            gotoexit.Visible = false;
            gotomainmenu.Visible = false;
            playregular.Visible = false;
            playhard.Visible = false;
            playsandbox.Visible = false;
            togglesound.Visible = false;
            resetstats.Visible = false;
            openbutton.Visible = false;
            closebutton.Visible = true;
            logo.Visible = false;
        }
    }
}