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
    class Menu
    {
        bool isgamesessionactive;
        bool menuisopen;
        bool scoresarevisible;

        Texture2D logo;
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

        public void StartGameSession()
        {
            isgamesessionactive = true;
        }

        public void EndGameSession()
        {
            isgamesessionactive = false;
        }

        public Menu(GraphicsDevice graphicsdevice, Point preferredscreensize, GameTextures texturecollection)
        {
            menuisopen = true;
            isgamesessionactive = false;
            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);


            openbutton = (new Button(new Vector2(preferredscreensize.X- 100,  100), texturecollection.GetTextures()["hud_menu_open"]));
            closebutton = (new Button(new Vector2(preferredscreensize.X-100, 100), texturecollection.GetTextures()["hud_menu_close"]));

            gotoplaymenu = (new Button(new Vector2(100, 0), texturecollection.GetTextures()["hud_menu_play"]));
            gotooptions = (new Button(new Vector2(100, 200), texturecollection.GetTextures()["hud_menu_options"]));
            gotoscores = (new Button(new Vector2(100, 400), texturecollection.GetTextures()["hud_menu_scores"]));
            gotoexit = (new Button(new Vector2(100, 600), texturecollection.GetTextures()["hud_menu_exit"]));

            gotomainmenu = (new Button(new Vector2(preferredscreensize.X - 100, preferredscreensize.Y - 400), texturecollection.GetTextures()["hud_menu_back"]));

            playregular = (new Button(new Vector2(100, 0), texturecollection.GetTextures()["hud_menu_play_normal"]));
            playhard = (new Button(new Vector2(100, 200), texturecollection.GetTextures()["hud_menu_play_hardmode"]));
            playsandbox = (new Button(new Vector2(100, 400), texturecollection.GetTextures()["hud_menu_play_sandbox"]));

            togglesound = (new Button(new Vector2(100, 0), texturecollection.GetTextures()["hud_menu_play_options_toggle"]));
            resetstats = (new Button(new Vector2(100, 200), texturecollection.GetTextures()["hud_menu_play_options_reset"]));
            OpenMainMenu();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isgamesessionactive&&!menuisopen)
            {
                openbutton.Draw(spriteBatch);
            }
            if (menuisopen)
            {
                if (isgamesessionactive)
                {
                    closebutton.Draw(spriteBatch);
                    spriteBatch.Draw(background, new Vector2(0, 0), new Color(0, 0, 0, 90));
                }
                gotoplaymenu.Draw(spriteBatch);
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
        }


        public void DrawScores(SpriteBatch spriteBatch,SpriteFont font, List<String> scores)
        {
            if (scoresarevisible)
            {
                for (int i = 0; i < scores.Count; i++)
                {
                    spriteBatch.DrawString(font, scores[i], new Vector2(100, 200 + 50 * i),Color.White);
                }
            }
        }

        public void OpenMainMenu()
        {
            menuisopen = true;
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
        }

        public void CloseMenu()
        {
            menuisopen = false;
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
            menuisopen = false;
        }   

        public void OpenPlayMenu()
        {
            menuisopen = true;
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
        }

        public void OpenOptions()
        {
            menuisopen = true;
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
        }

        public void OpenScores()
        {
            menuisopen = true;
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
        }
    }
}