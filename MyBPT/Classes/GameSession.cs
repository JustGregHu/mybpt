using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Mono.Data.Sqlite;

#if DEBUG
//debug function
#endif

namespace MyBPT.Classes
{
    /// <summary>
    /// Játékmenetet kezelő objektum. Irányítja, tárolja és kezeli az összes játékmenethez szükséges funkciót és objektumot
    /// </summary>
    public class GameSession : Game
    {
        #region Változók
        //Számolás
        FrameCounter frameCounter;
        IsoCalculator isoCalculator;
        Perlin perlin;
        CountDown incometimer;
        CountDown gametimer;
        CountDown messagetimer;

        //Megjelenítés
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatchHud;
        SpriteBatch spriteBatchMenu;
        SpriteBatch spriteBatchKeyboard;
        SpriteFont font;
        GameTextures texturecollection;

        //Játékos
        Camera camera;
        Vector2 isometriccameraposition;
        Camera hud;
        TouchCollection tc;
        Player player;
        int dpadmoveinterval;
        int dpadmoveintervalinitial;

        //Játékvilág
        GameWorld gameworld;
        bool hardmode;
        bool sandbox;

        //Gombok, menük
        Menu menu;
        BuyMenu buymenu;
        Button zoomin;
        Button zoomout;
        Button dpad_up;
        Button dpad_down;
        Button dpad_left;
        Button dpad_right;
        string gamemessage;
        OnScreenKeyboard onscreenkeyboard;

        //Változók
        bool isgamesessionactive;
        int hudmargin;
        bool movingbuilding;
        bool buyingbuilding;
        bool gamehasended;
        int highlightedstationid;
        int highlightedobstacleid;
        Point preferredscreensize;
        Point tileSize;
        float viewdistance;
        int stationcost;
        int terminuscost;
        int lengthofincomeinterval;
        int lengthofgametime;
        int lengthofmessageshow;
        int monthcount;
        int currentyear;

        //HUD textúrák
        Texture2D gameplaystats_coins;
        Texture2D gameplaystats_clock;
        Texture2D gameplaystats_calendar;
        Texture2D gameplaystats_influence;
        Texture2D gameplaystats_level;

        #endregion
        #region MonoGame főfunkciók

        /// <summary>
        /// A játékmenet futtatásához elengedhetetlen funckiókat tölt be
        /// </summary>
        public GameSession()
        {
            //Initiates graphics, loads content file (throws if a nonexistant texture is being referenced)
            preferredscreensize = new Point(1280, 720);
            graphics = new GraphicsDeviceManager(this);
            try { Content.RootDirectory = "Content"; }
            catch (Exception) { }
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = preferredscreensize.X;
            graphics.PreferredBackBufferHeight = preferredscreensize.Y;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Inicializálja a játéktmenethez szükséges változókat és funkciókat. Ez a játékmenet létrehozásakor azonnal lefut
        /// </summary>
        protected override void Initialize()
        {   
            //Megjelenítés
            DeactivateGame();
            tileSize = new Point(200, 100);
            hudmargin = 50;
            viewdistance = 2.2f;

            //Számolás
            frameCounter = new FrameCounter();
            isoCalculator = new IsoCalculator();
            perlin = new Perlin();
            incometimer = new CountDown();
            gametimer = new CountDown();
            messagetimer = new CountDown();
            gamemessage = "";
            messagetimer.StartTimer(1);

            //Játékos
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            font = Content.Load<SpriteFont>("regulartext");
            camera = new Camera(GraphicsDevice.Viewport);
            hud = new Camera(GraphicsDevice.Viewport);
            dpadmoveintervalinitial = 10;
            dpadmoveinterval = dpadmoveintervalinitial;

            //Játékvilág
            stationcost = 350;
            terminuscost = 1000;

            base.Initialize();
        }

        /// <summary>
        /// Inicializálás után fut, textúrákat, gombokat tölt be és helyez el
        /// </summary>
        protected override void LoadContent()
        {
            //Textures, SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHud = new SpriteBatch(GraphicsDevice);
            spriteBatchMenu = new SpriteBatch(GraphicsDevice);
            spriteBatchKeyboard = new SpriteBatch(GraphicsDevice);
            texturecollection = new GameTextures();
            LoadTexturesIntoTextureCollection();

            //Buttons, Menus
            Vector2 initialpos = new Vector2(0, 0);
            menu = new Menu(GraphicsDevice, preferredscreensize, texturecollection, hudmargin);
            buymenu = new BuyMenu(GraphicsDevice, preferredscreensize, texturecollection, hudmargin);

            int zoomwidth = texturecollection.GetTextures()["hud_zoom_in"].Width;
            zoomin = new Button(initialpos, texturecollection.GetTextures()["hud_zoom_in"]);
            zoomout = new Button(initialpos, texturecollection.GetTextures()["hud_zoom_out"]);
            dpad_left = new Button(initialpos, texturecollection.GetTextures()["hud_dpad_west"]);
            dpad_right = new Button(initialpos, texturecollection.GetTextures()["hud_dpad_east"]);
            dpad_up = new Button(initialpos, texturecollection.GetTextures()["hud_dpad_north"]);
            dpad_down = new Button(initialpos, texturecollection.GetTextures()["hud_dpad_south"]);
            gameplaystats_coins = texturecollection.GetTextures()["hud_gameplaystats_coins"];
            gameplaystats_clock = texturecollection.GetTextures()["hud_gameplaystats_clock"];
            gameplaystats_calendar = texturecollection.GetTextures()["hud_gameplaystats_calendar"];
            gameplaystats_influence = texturecollection.GetTextures()["hud_gameplaystats_influence"];
            gameplaystats_level = texturecollection.GetTextures()["hud_gameplaystats_level"];

            zoomin.UpdatePosition(new Vector2(preferredscreensize.X / 2 - zoomwidth - 20, preferredscreensize.Y - zoomin.Texture.Height - hudmargin));
            zoomout.UpdatePosition(new Vector2(preferredscreensize.X / 2 + 20, preferredscreensize.Y - zoomout.Texture.Height - hudmargin));
            dpad_left.UpdatePosition(new Vector2(hudmargin, preferredscreensize.Y - 200 - dpad_left.Texture.Height / 2));
            dpad_right.UpdatePosition(new Vector2(100 + hudmargin, preferredscreensize.Y - 100 - dpad_left.Texture.Height / 2));
            dpad_up.UpdatePosition(new Vector2(100 + hudmargin, preferredscreensize.Y - 200 - dpad_left.Texture.Height / 2));
            dpad_down.UpdatePosition(new Vector2(hudmargin, preferredscreensize.Y - 100 - dpad_left.Texture.Height / 2));

            onscreenkeyboard = new OnScreenKeyboard(GraphicsDevice, preferredscreensize, texturecollection);
        }

        /// <summary>
        /// A menüben játékmód kiválasztása után fut. Inicializálja a jelenlegi játék alkalom funckióit, pályát generál, betölti a játékszabályokat a megadott változók alapján
        /// </summary>
        /// <param name="issandbox">Ha igaz, időzítő és mentés nélkül indul el a játék</param>
        /// <param name="size">Ha igaz, az általánosnál kisebb pálya generálódik</param>
        /// <param name="isupgradable">Ha igaz, engedélyezett az állomások felújítása</param>
        private void InitializeSession(bool isupgradable, bool size, bool issandbox)
        {
            //Játékmenet, logika
            movingbuilding = false;
            buyingbuilding = false;
            gamehasended = false;

            //Játékos
            player = new Player("", 2000, 1);

            //Terep generálás
            gameworld = new GameWorld(texturecollection, size);
            hardmode = isupgradable;
            sandbox = issandbox;
            gameworld.GenerateMap(spriteBatch, preferredscreensize, GraphicsDevice);
            gameworld.InitiateHighlightTile();
            SnapCameraToSelectedTile();

            //Játékvilág
            InitializeYear();
            InitializeMonth();
            if (!issandbox)
            {
                lengthofgametime = 720;
                gametimer.StartTimer(lengthofgametime);
            }
            lengthofincomeinterval = 30;
            incometimer.StartTimer(lengthofincomeinterval);

            menu.CloseMenu();
        }

        /// <summary>
        /// Akkor fut, ha az appból kilép a játékos
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Inicializálás és betöltés után fut. A saját tempójában frissül amilyen gyakran lehetséges. Itt frissülnek a játékmenet azon változói, funkciói amelyek nem a megjelenítést szolgálják.
        /// </summary>
        /// <param name="gameTime">MonoGame objektum, amely segít lépést tartani a játék futási idejével</param>
        protected override void Update(GameTime gameTime)
        {
            //Game Over esemény
            if (isgamesessionactive && !sandbox && gametimer.Timeleft < 1)
            {
                gamehasended = true;
                CloseMenu();
                CloseBuyMenu();
                onscreenkeyboard.OpenKeyboard();
            }
            UpdateTouchCollection();
            //Játékalkalmon belüli események
            if (isgamesessionactive)
            {
                //Segít a játékalkalomnak lépést tartani a játékos cselekvéseivel
                PayIncome();
                CheckIfAnyStationsAreHighlighted();
                CheckIfAnyObstaclesAreHighlighted();
                CheckIfAnyBuildingsAreHighlighted();
                UpdateHudVisibilityBasedOnBuyMenu();
                UpdatePlayerLevel();
                //HUD események és gombnyomások lekezelése
                if (tc.Count > 0)
                {
                    if (!gamehasended)
                    {
                        TouchLocation currenttouchlocation = tc[0];
                        HandleZoomButtonPresses(currenttouchlocation);
                        HandleDpadPresses(currenttouchlocation);
                        HandleBuyMenuButtonPresses(currenttouchlocation);
                        HandleStationButtonPresses(currenttouchlocation);
                        HandleObstacleDemolishButtonPresses(currenttouchlocation);
                        HandleBuildingDemolishButtonPresses(currenttouchlocation);
                    }
                }
            }
            //Menü gomblenyomások
            if (tc.Count > 0)
            {
                TouchLocation currenttouchlocation = tc[0];
                HandleMenuButtonPresses(currenttouchlocation);
                HandleOnScreenKeyboardButtonPresses(currenttouchlocation);
            }

            //Kamera helyzetének frissítése
            camera.Update(gameTime, tc);

            
            ExitAppIfBackButtonIsPressed();
            base.Update(gameTime);
        }

        /// <summary>
        /// Inicializálás és betöltés után fut. A saját tempójában frissül amilyen gyakran lehetséges, bár az Update-nál lassabban. Itt frissülnek a játékmenet azon változói, funkciói amelyek a megjelenítést szolgálják.
        /// </summary>
        /// <param name="gameTime">MonoGame objektum, amely segít lépést tartani a játék futási idejével</param>
        protected override void Draw(GameTime gameTime)
        {
            //Terep rajzolása, ha éppen látható
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform);
            if (isgamesessionactive)
            {
                UpdateIsometricCamera();
                DrawMap();
                DrawRoads();
                UpdateHighlightedTile();
                DrawObstacles();
                DrawStations();
                DrawBuildings();
            }
            spriteBatch.End();

            //Játékalkalmi információk, kezelőfelület rajzolása, ha éppen látható
            if (isgamesessionactive)
            {

                spriteBatchHud.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        RasterizerState.CullNone);
                DrawMainHud();
                DrawBuyMenuElements();
                DrawObstacleDemolitionMenu();
                DrawBuildingDemolitionMenu();
                DrawGameMessage();
                spriteBatchHud.End();

            }

            //Menü megrajzolása, ha éppen látható
            spriteBatchMenu.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        RasterizerState.CullNone);

            DrawMenuElements();
            spriteBatchMenu.End();

            //Billentyűzet megrajzolása, ha éppel látható
            spriteBatchKeyboard.Begin(SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);
            DrawOnScreenKeyboard();
            spriteBatchKeyboard.End();

            base.Draw(gameTime);
        }

        #endregion 
        #region Játékos, Kamera, Érintés

        /// <summary>
        /// Inicializálja a MonoGame érintésgyüjteményt. Ezzel megszakíthatóak a touch gesture-k
        /// </summary>
        private void ReInitiateTouchCollection()
        {
            tc = new TouchCollection();
            tc = TouchPanel.GetState();
        }

        /// <summary>
        /// Frissíti az esetleges érintés pozícióját
        /// </summary>
        private void UpdateTouchCollection()
        {
            tc = TouchPanel.GetState();
        }

        /// <summary>
        /// A kurzorra helyezi a játékbei kamerát
        /// </summary>
        private void SnapCameraToSelectedTile()
        {
            Tile currenttile = gameworld.MapData[gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y];
            float mapx = currenttile.Position.X - preferredscreensize.X / 2 + currenttile.Texture.Width / 2;
            float mapy = currenttile.Position.Y - preferredscreensize.Y / 2 + currenttile.Texture.Height / 2; ;
            camera.TargetPosition = (new Vector2(mapx, mapy));
        }

        /// <summary>
        /// A játékbeli kamera pozíciójának frissítése
        /// </summary>
        private void UpdateIsometricCamera()
        {
            isometriccameraposition = isoCalculator.IsoTo2D(new Vector2(-camera.Position.X * 2, camera.Position.Y));
        }

        #endregion
        #region Kurzor, Kiemelt Csempe

        /// <summary>
        /// Frissíti a kurzor helyzetét
        /// </summary>
        private void UpdateHighlightedTile()
        {
            gameworld.HighlightCurrentTile(spriteBatch);
        }

        /// <summary>
        /// Ellenőrzi, hogy a kurozor pozícióján található-e állomás vagy végállomás
        /// </summary>
        private void CheckIfAnyStationsAreHighlighted()
        {
            for (int i = 0; i < gameworld.Stations.Count; i++)
            {
                gameworld.Stations[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }

        /// <summary>
        /// Ellenőrzi, hogy a kurozor pozícióján található-e épület
        /// </summary>
        private void CheckIfAnyBuildingsAreHighlighted()
        {
            for (int i = 0; i < gameworld.Buildings.Count; i++)
            {
                gameworld.Buildings[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }

        /// <summary>
        /// Ellenőrzi, hogy a kurozor pozícióján található-e blokád
        /// </summary>
        private void CheckIfAnyObstaclesAreHighlighted()
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                gameworld.Obstacles[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }

        /// <summary>
        /// Ellenőrzi, hogy a kurozor pozícióján található-e útobjektum
        /// </summary>
        private void CheckIfAnyRoadsAreHighlighted()
        {
            for (int i = 0; i < gameworld.Roads.Count; i++)
            {
                gameworld.IsThereARoadAt(gameworld.CurrentTilePosition);
            }
        }

        /// <summary>
        /// Vissza adja a kurzor pocízióján található állomás azonosítóját. -1, ha nem talált a pozíción állomást
        /// </summary>
        private int IdOfHighlightedStation
        {
            get
            {
                for (int i = 0; i < gameworld.Stations.Count; i++)
                {
                    if (gameworld.Stations[i].Highlighted)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        #endregion
        #region Rajzolás

        /// <summary>
        /// Megjeleníti a terepet (a csempéket)
        /// </summary>
        private void DrawMap()
        {
            try
            {
                for (int i = RemoveOffsetMin((int)(((isometriccameraposition.X - (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X))); i < RemoveOffsetMax((int)((((isometriccameraposition.X + graphics.PreferredBackBufferWidth + tileSize.X) + (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X)), gameworld.Worldsize); i++)
                {

                    for (int p = RemoveOffsetMin((int)(((isometriccameraposition.Y - (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X))); p < RemoveOffsetMax((int)((((isometriccameraposition.Y + graphics.PreferredBackBufferWidth + tileSize.X) + (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X)), gameworld.Worldsize); p++)
                    {
                        gameworld.MapData[i, p].Draw(spriteBatch);
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Megjeleníti az összes állomást
        /// </summary>
        private void DrawStations()
        {
            for (int i = 0; i < gameworld.Stations.Count; i++)
            {
                gameworld.Stations[i].DrawAffectionOfArea(spriteBatch);
            }
            for (int i = 0; i < gameworld.Stations.Count; i++)
            {
                gameworld.Stations[i].Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Megjeleníti az összes épületet
        /// </summary>
        private void DrawBuildings()
        {
            for (int i = 0; i < gameworld.Buildings.Count; i++)
            {
                gameworld.Buildings[i].Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Megjeleníti az összes blokádot
        /// </summary>
        private void DrawObstacles()
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                gameworld.Obstacles[i].Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Megjeleníti az összes útobjektumot
        /// </summary>
        private void DrawRoads()
        {
            for (int i = 0; i < gameworld.Roads.Count; i++)
            {
                gameworld.Roads[i].Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Megjeleníti a nagyításra, kicsinítésre használt két gombot 
        /// </summary>
        private void DrawZoomButtons()
        {
            if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                zoomin.Draw(spriteBatchHud);
                zoomout.Draw(spriteBatchHud);
            }
        }

        /// <summary>
        /// Megjeleníti a directional pad-et, amely segítségével a játékos által mozgathatóvá válik a kurzor
        /// </summary>
        private void DrawDpad()
        {
            if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                dpad_up.Draw(spriteBatchHud);
                dpad_right.Draw(spriteBatchHud);
                dpad_down.Draw(spriteBatchHud);
                dpad_left.Draw(spriteBatchHud);
            }
        }

        /// <summary>
        /// Megjeleníti a menükön kívül található játékmenetbeli információkat és az állomásokhoz tartozó funkciók gombjait
        /// </summary>
        private void DrawMainHud()
        {
            if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                DrawZoomButtons();
                DrawDpad();
                int idofhighlightedstation = IdOfHighlightedStation;
                if (idofhighlightedstation > -1)
                {
                    gameworld.Stations[idofhighlightedstation].DrawButtons(spriteBatchHud);
                    gameworld.Stations[idofhighlightedstation].DrawInfo(spriteBatchHud, font, hudmargin);
                }
                else
                {
                    spriteBatchHud.Draw(gameplaystats_calendar, new Vector2(hudmargin, hudmargin), Color.White);
                    spriteBatchHud.Draw(gameplaystats_clock, new Vector2(hudmargin, hudmargin + 50), Color.White);
                    spriteBatchHud.Draw(gameplaystats_coins, new Vector2(hudmargin, hudmargin + 100), Color.White);
                    spriteBatchHud.Draw(gameplaystats_level, new Vector2(hudmargin, hudmargin + 150), Color.White);

                    spriteBatchHud.DrawString(font, currentyear + " " + CurrentMonth, new Vector2(hudmargin * 2, hudmargin), Color.White);
                    spriteBatchHud.DrawString(font, incometimer.Timeleft + "s", new Vector2(hudmargin * 2, hudmargin + 50), Color.White);
                    spriteBatchHud.DrawString(font, player.Money.ToString(), new Vector2(hudmargin * 2, hudmargin + 100), Color.White);
                    spriteBatchHud.DrawString(font, player.Level.ToString(), new Vector2(hudmargin * 2, hudmargin + 150), Color.White);

                }
            }
        }

        /// <summary>
        /// Megjeleníti a menüt, ha nyitva van
        /// </summary>
        private void DrawMenuElements()
        {
            if (!buymenu.BuymenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                menu.Draw(spriteBatchMenu, font);
                if (menu.ScoresAreVisible)
                {
                    List<ScoreInstance> scores = GetAllScores();
                    List<String> scorestrings = new List<string>();
                    for (int i = 0; i < scores.Count; i++)
                    {
                        scorestrings.Add(scores[i].Playername + ": " + scores[i].Amount+" total income on " + scores[i].Timestamp);
                    }
                    menu.DrawScores(spriteBatchMenu, font, scorestrings);
                }

            }
        }

        /// <summary>
        /// Megjeleníti a vásárlás menüt, ha nyitva van
        /// </summary>
        private void DrawBuyMenuElements()
        {
            if (!movingbuilding && (!menu.MenuIsOpen) && !onscreenkeyboard.KeyboardIsOpen)
            {
                buymenu.Draw(spriteBatchHud, gameworld, font);
            }
        }

        /// <summary>
        /// Megjeleníti a blokád eltörlésére használt gombot, ha a kurzor egy blokádon található
        /// </summary>
        private void DrawObstacleDemolitionMenu()
        {
            if (!movingbuilding)
                if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
                {
                    for (int i = 0; i < gameworld.Obstacles.Count; i++)
                    {
                        gameworld.Obstacles[i].DrawButtons(spriteBatchHud);
                    }
                }


        }

        /// <summary>
        /// Megjeleníti az épületek eltörlésére használt gombot, ha a kurzor egy épületen található
        /// </summary>
        private void DrawBuildingDemolitionMenu()
        {
            if (!movingbuilding)
                if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
                {
                    for (int i = 0; i < gameworld.Buildings.Count; i++)
                    {
                        gameworld.Buildings[i].DrawButtons(spriteBatchHud);
                    }
                }
        }

        /// <summary>
        /// Megjeleníti a képrenyőbeli bilentyűzetet, ha a játék véget ér
        /// </summary>
        private void DrawOnScreenKeyboard()
        {
            onscreenkeyboard.DrawOnScreenKeyboard(spriteBatchKeyboard, font);
            if (gamehasended && isgamesessionactive)
            {
                spriteBatchKeyboard.DrawString(font, "Game Over! Your total income is: " + gameworld.CurrentIncome, new Vector2(hudmargin + 320, hudmargin + 150), Color.White);
                spriteBatchKeyboard.DrawString(font, "Please enter your name below.", new Vector2(hudmargin + 350, hudmargin + 190), Color.White);
            }

        }

        /// <summary>
        /// Megjelenít egy pirosbetűs játéküzenetet, ha az inicializálva van, és még nem járt le a megjelenítési ideje
        /// </summary>
        private void DrawGameMessage()
        {
            if (messagetimer.Timeleft > 0)
            {
                spriteBatchHud.DrawString(font, gamemessage, new Vector2(preferredscreensize.X - preferredscreensize.X / 2, preferredscreensize.Y - preferredscreensize.Y/4), Color.Red);
            }
        }

        /// <summary>
        /// Megjeleníti az FPS számlálót
        /// </summary>
        private void DrawFPS(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);
            spriteBatchHud.DrawString(font, fps, new Vector2(0, 0), Color.White);
        }

        #endregion
        #region Állomás elhelyezése, Megvásárlása

        /// <summary>
        /// Megkezdi az állomás mozgatásának folyamatát
        /// </summary>
        private void BeginStationPlacement()
        {
            movingbuilding = true;
            gameworld.Stations[highlightedstationid].InitiateMove();
            gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
        }

        /// <summary>
        /// Tudatja a játékmenettel, hogy a jelenleg kiválasztott állomás egy újonnan vásárlandó állomás, majd megkezdi a mozgatási folyamatát
        /// </summary>
        /// <param name="isterminus">Ha igaz, a jelenlegi állomás egy végállomás</param>
        private void MarkPlacementForPurchase(bool isterminus)
        {
            highlightedstationid = gameworld.Stations.Count;
            if (isterminus)
            {
                gameworld.Stations.Add(new Station(GraphicsDevice, preferredscreensize, texturecollection, hudmargin, gameworld, new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y), true));
            }
            else
            {
                gameworld.Stations.Add(new Station(GraphicsDevice, preferredscreensize, texturecollection, hudmargin, gameworld, new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y), false));

            }
            BeginStationPlacement();
            buyingbuilding = true;
        }

        /// <summary>
        /// Megpróbálja véglegesíteni az állomás elhelyezésének folyamatát. Ha az új pozíció nem felel meg, értesíti arról a játékost
        /// </summary>
        private void FinishStationPlacement()
        {
            if (!gameworld.IsThereanObstacleAt(gameworld.Stations[highlightedstationid].Coordinates))
            {
                if (!gameworld.IsThereWaterAt(gameworld.Stations[highlightedstationid].Coordinates))
                {
                    if (!gameworld.IsThereARoadAt(gameworld.Stations[highlightedstationid].Coordinates))
                    {
                        if (gameworld.IsThereARoadNextTo(gameworld.Stations[highlightedstationid].Coordinates, 1))
                        {
                            if (!gameworld.IsThereABuildingAt(gameworld.Stations[highlightedstationid].Coordinates))
                            {
                                gameworld.Stations[highlightedstationid].Highlighted = false;
                                gameworld.Stations[highlightedstationid].FinalizeMove(gameworld);
                                movingbuilding = false;
                                buyingbuilding = false;
                            }
                            else
                            {
                                InitializeGameMessage("You cannot place the station onto a building!");
                            }
                        }
                        else
                        {
                            InitializeGameMessage("Should place the station next to a road!");
                        }
                    }
                    else
                    {
                        InitializeGameMessage("You cannot place a station onto the road!");
                    }
                }
                else
                {
                    InitializeGameMessage("You cannot place the station on the water!");
                }
            }
            else
            {
                InitializeGameMessage("There's an obstacle in the way!");
            }
        }

        /// <summary>
        /// Megpróbálja véglegesíteni az új állomás megvásárlásának folyamatát. Ha az új pozíció nem felel meg, értesíti arról a játékost
        /// </summary>
        private void MakeStationPurchase()
        {
            if (!gameworld.IsThereanObstacleAt(gameworld.Stations[highlightedstationid].Coordinates))
            {
                if (!gameworld.IsThereWaterAt(gameworld.Stations[highlightedstationid].Coordinates))
                {
                    if (!gameworld.IsThereARoadAt(gameworld.Stations[highlightedstationid].Coordinates))
                    {
                        if (gameworld.IsThereARoadNextTo(gameworld.Stations[highlightedstationid].Coordinates, 1))
                        {
                            if (!gameworld.IsThereABuildingAt(gameworld.Stations[highlightedstationid].Coordinates))
                            {
                                player.AddMoney(-gameworld.Stations[highlightedstationid].Cost);
                                FinishStationPlacement();
                                buyingbuilding = false;
                            }
                            else
                            {
                                InitializeGameMessage("You cannot place the station onto a building!");
                            }
                        }
                        else
                        {
                            InitializeGameMessage("Should place the station next to a road!");
                        }
                    }
                    else
                    {
                        InitializeGameMessage("You cannot place a station onto the road!");
                    }
                }
                else
                {
                    InitializeGameMessage("You cannot place the station on the water!");
                }
            }
            else
            {
                InitializeGameMessage("There's an obstacle in the way!");
            }
        }

        /// <summary>
        /// Az állomás mozgatási folyamatának visszavonása, az épület visszakerül az eredeti pozíciójára
        /// </summary>
        private void UndoStationPlacement()
        {
            gameworld.Stations[highlightedstationid].Highlighted = false;
            gameworld.Stations[highlightedstationid].RollbackMove();
            movingbuilding = false;
            buyingbuilding = false;
        }

        /// <summary>
        /// Az állomás mozgazási folyamatának és megvásárlásának visszavonása. A játékos visszakapja a pénzét, az állomás pedig törlődik
        /// </summary>
        private void CancelStationPurchase()
        {
            UndoStationPlacement();
            gameworld.Stations.RemoveAt(highlightedstationid);
        }

        /// <summary>
        /// A kurzor alatt található állomás eladása. A játékos visszakapja az állomás jelenlegi megvásárlási értékének a felét
        /// </summary>
        private void SellHighlightedStation()
        {
            player.AddMoney(gameworld.Stations[highlightedstationid].SellPrice);
            gameworld.Stations.RemoveAt(highlightedstationid);
            highlightedstationid = -1;
        }

        /// <summary>
        /// A kurzor alatt található állomás továbbfejlesztése. Ez pénzbe kerül a játékosnak
        /// </summary>
        private void UpgradeHighlightedStation()
        {
            if (!hardmode)
            {
                if (gameworld.Stations[highlightedstationid].Level != gameworld.Stations[highlightedstationid].Maxlevel)
                {
                    if (player.CanAfford(gameworld.Stations[highlightedstationid].UpgradeCost))
                    {
                        gameworld.Stations[highlightedstationid].LevelStationUp(texturecollection, gameworld);
                    }
                    else
                    {
                        InitializeGameMessage("Can't afford the upgrade!");
                    }
                }
                else
                {
                    InitializeGameMessage("Building is already at max level!");
                }

                player.AddMoney(-gameworld.Stations[highlightedstationid].UpgradeCost);
                gameworld.Stations[highlightedstationid].LevelStationUp(texturecollection, gameworld);
            }
            else
            {
                InitializeGameMessage("No upgrades allowed on hardmode!");
            }
        }

        /// <summary>
        /// A kurzor alatt található blokád eltávolítása. Ez pénzbe kerül a játékosnak
        /// </summary>
        private void DemolishHighlightedObstacle()
        {
            player.AddMoney(-gameworld.Obstacles[highlightedobstacleid].Cost);
            gameworld.Obstacles.RemoveAt(highlightedobstacleid);
            highlightedobstacleid = -1;
        }

        /// <summary>
        /// A kurzor alatt található épület eltávolítása. Ez pénzbe kerül a játékosnak
        /// </summary>
        private void DemolishHighlightedBuilding()
        {
            player.AddMoney(-gameworld.Buildings[highlightedobstacleid].Influenceamount);
            gameworld.Buildings.RemoveAt(highlightedobstacleid);
            for (int i = 0; i < gameworld.Stations.Count; i++)
            {
                gameworld.Stations[i].ApplyIncome(gameworld);
            }
            highlightedobstacleid = -1;
        }

        #endregion
        #region Állomás mozgatása

        /// <summary>
        /// Egy csempe távolságnyira Észak fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void MoveStationUp()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X - 1, gameworld.Stations[highlightedstationid].Coordinates.Y)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X - 1, gameworld.Stations[highlightedstationid].Coordinates.Y), gameworld);
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
                InitializeGameMessage("There's a station in the way!");
            }

        }

        /// <summary>
        /// Egy csempe távolságnyira Dél fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void MoveStationDown()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X + 1, gameworld.Stations[highlightedstationid].Coordinates.Y)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X + 1, gameworld.Stations[highlightedstationid].Coordinates.Y ), gameworld);
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
                InitializeGameMessage("There's a station in the way!");

            }
        }

        /// <summary>
        /// Egy csempe távolságnyira Nyugat fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void MoveStationLeft()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y - 1)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                 gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y - 1), gameworld);;
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
                InitializeGameMessage("There's a station in the way!");

            }
        }

        /// <summary>
        /// Egy csempe távolságnyira Kelet fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void MoveStationRight()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y + 1)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y + 1), gameworld);
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
                InitializeGameMessage("There's a station in the way!");

            }
        }

        #endregion
        #region Gomblenyomások eseményei

        /// <summary>
        /// A kamerát közelített állapotba helyezi
        /// </summary>
        private void ZoomIn()
        {
            camera.Zoom = 1f;
            viewdistance = 1.1f;
        }

        /// <summary>
        /// A kamerát távolított állapotba helyezi
        /// </summary>
        private void ZoomOut()
        {
            camera.Zoom = 0.5f;
            viewdistance = 2.2f;
        }

        /// <summary>
        /// Egy csempe távolságnyira Észak fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void DPAD_Up()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X - 1, gameworld.CurrentTilePosition.Y));
            SnapCameraToSelectedTile();
        }

        /// <summary>
        /// Egy csempe távolságnyira Dél fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void DPAD_Down()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X + 1, gameworld.CurrentTilePosition.Y));
            SnapCameraToSelectedTile();
        }

        /// <summary>
        /// Egy csempe távolságnyira Nyugat fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void DPAD_Left()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y - 1));
            SnapCameraToSelectedTile();
        }

        /// <summary>
        /// Egy csempe távolságnyira Kelet fele helyezi a jelenleg kiválasztott állomást
        /// </summary>
        private void DPAD_Right()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y + 1));
            SnapCameraToSelectedTile();
        }

        /// <summary>
        /// A nagyításra használt gombesemények kezelése. Csak akkor működnek, ha gombok láthatóak
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleZoomButtonPresses(TouchLocation currenttouchlocation)
        {
            if (zoomin.IsTapped(currenttouchlocation) && zoomin.Visible)
            {
                if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
                {
                    ZoomIn();
                }
            }
            if (zoomout.IsTapped(currenttouchlocation) && zoomout.Visible)
            {
                if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
                {
                    ZoomOut();
                }
            }
        }

        /// <summary>
        /// A kurzor mozgatására használt gombesemények kezelése. Csak akkor működnek, ha gombok láthatóak
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleDpadPresses(TouchLocation currenttouchlocation)
        {
            if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                if (dpad_up.Visible)
                {
                    if (dpad_up.IsHeld(currenttouchlocation))
                    {
                        if (dpadmoveinterval == dpadmoveintervalinitial)
                        {
                            DPAD_Up();
                            if (movingbuilding)
                            {
                                MoveStationUp();
                            }
                        }

                        DpadMoveIntervalNextTick();
                    }
                    else
                    {
                        if (dpad_up.IsTapped(currenttouchlocation))
                            InitializeDpadMoveInterval();
                    }
                }
                if (dpad_down.Visible)
                {
                    if (dpad_down.IsHeld(currenttouchlocation))
                    {
                        if (dpadmoveinterval == dpadmoveintervalinitial)
                        {
                            DPAD_Down();
                            if (movingbuilding)
                            {
                                MoveStationDown();
                            }
                        }

                        DpadMoveIntervalNextTick();
                    }
                    else
                    {
                        if (dpad_down.IsTapped(currenttouchlocation))
                            InitializeDpadMoveInterval();
                    }
                }
                if (dpad_left.Visible)
                {
                    if (dpad_left.IsHeld(currenttouchlocation))
                    {
                        if (dpadmoveinterval == dpadmoveintervalinitial)
                        {
                            DPAD_Left();
                            if (movingbuilding)
                            {
                                MoveStationLeft();
                            }
                        }

                        DpadMoveIntervalNextTick();
                    }
                    else
                    {
                        if (dpad_left.IsTapped(currenttouchlocation))
                            InitializeDpadMoveInterval();
                    }
                }
                if (dpad_right.Visible)
                {
                    if (dpad_right.IsHeld(currenttouchlocation))
                    {
                        if (dpadmoveinterval == dpadmoveintervalinitial)
                        {
                            DPAD_Right();
                            if (movingbuilding)
                            {
                                MoveStationRight();
                            }
                        }

                        DpadMoveIntervalNextTick();
                    }
                    else
                    {
                        if (dpad_right.IsTapped(currenttouchlocation))
                            InitializeDpadMoveInterval();
                    }
                }
            }
            
        }

        /// <summary>
        /// A főmenü navígálására használt gombesemények kezelése. Csak akkor működnek, ha a főmenü látható
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleMenuButtonPresses(TouchLocation currenttouchlocation)
        {
            if (menu.GoToPlaymenuButton.IsTapped(currenttouchlocation)&&menu.GoToPlaymenuButton.Visible)
            {
                currenttouchlocation = new TouchLocation();
                OpenPlayMenu();
            }
            if (menu.GoToOptionsButton.IsTapped(currenttouchlocation) && menu.GoToOptionsButton.Visible)
            {
                currenttouchlocation = new TouchLocation();
                OpenOptions();
            }
            if (menu.GoToScoresButton.IsTapped(currenttouchlocation) && menu.GoToScoresButton.Visible)
            {
                OpenScores();
            }
            if (menu.GoToExitButton.IsTapped(currenttouchlocation) && menu.GoToExitButton.Visible)
            {
                Exit();
            }
            if (menu.GoToMainmenuButton.IsTapped(currenttouchlocation) && menu.GoToMainmenuButton.Visible)
            {
                OpenMainMenu();
            }
            if (menu.PlayRegularButton.IsTapped(currenttouchlocation) && menu.PlayRegularButton.Visible)
            {
                StartRegularSession();
            }
            if (menu.PlayHardButton.IsTapped(currenttouchlocation) && menu.PlayHardButton.Visible)
            {
                StartHardSession();
            }
            if (menu.PlaySandboxButton.IsTapped(currenttouchlocation) && menu.PlaySandboxButton.Visible)
            {
                StartSandboxSession();
            }
            if (menu.ToggleSoundButton.IsTapped(currenttouchlocation) && menu.ToggleSoundButton.Visible)
            {
                ToggleSound();
            }
            if (menu.ResetStatsButton.IsTapped(currenttouchlocation) && menu.ResetStatsButton.Visible)
            {
                ResetStats();
            }

            if (menu.OpenButton.IsTapped(currenttouchlocation) && menu.OpenButton.Visible)
            {
                if (!buymenu.BuymenuIsOpen)
                {
                    currenttouchlocation = new TouchLocation();
                    OpenMainMenu();
                }
                
            }
            if (menu.CloseButton.IsTapped(currenttouchlocation) && menu.CloseButton.Visible)
            {
                if (!buymenu.BuymenuIsOpen)
                {
                    currenttouchlocation = new TouchLocation();
                    CloseMenu();
                }
            }
        }

        /// <summary>
        /// A vásárlás menü navígálására használt gombesemények kezelése. Csak akkor működnek, ha a vásárlás menü látható
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleBuyMenuButtonPresses(TouchLocation currenttouchlocation)
        {
            if (buymenu.OpenButton.IsTapped(currenttouchlocation) && buymenu.OpenButton.Visible)
            {
                if (!gameworld.IsThereAStationAt(gameworld.CurrentTilePosition))
                {
                    currenttouchlocation = new TouchLocation();
                    OpenBuyMenu();
                }
                else
                {
                    InitializeGameMessage("There's a station in the way!");
                }

            }
            if (buymenu.CloseButton.IsTapped(currenttouchlocation) && buymenu.CloseButton.Visible)
            {
                currenttouchlocation = new TouchLocation();
                CloseBuyMenu();
                SnapCameraToSelectedTile();
            }
            if (buymenu.BuymenuIsOpen)
            {
                if (buymenu.TerminusBuyButton.IsTapped(currenttouchlocation) && buymenu.TerminusBuyButton.Visible)
                {
                    if (player.CanAfford(terminuscost))
                    {
                        if (gameworld.IsLevelingUpAllowed())
                        {
                            CloseBuyMenu();
                            SnapCameraToSelectedTile();
                            MarkPlacementForPurchase(true);
                        }
                        else
                        {
                            InitializeGameMessage("You need to build more stations!");
                        }

                    }
                    else
                    {
                        InitializeGameMessage("Can't afford terminus!");
                    }
                }
                if (buymenu.StationBuyButton.IsTapped(currenttouchlocation) && buymenu.StationBuyButton.Visible)
                {
                    if (player.CanAfford(stationcost))
                    {
                        CloseBuyMenu();
                        SnapCameraToSelectedTile();
                        MarkPlacementForPurchase(false);
                    }
                    else
                    {
                        InitializeGameMessage("Can't afford station!");
                    }
                }
            }
        }

        /// <summary>
        /// Az állomásokhoz tartozó funkciók gombeseményeinek kezelése. Csak akkor működnek, ha nincsenek menük nyitva és a kurzor egy állomáson tartózkodik
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleStationButtonPresses(TouchLocation currenttouchlocation)
        {
            for (int i = 0; i < gameworld.Stations.Count; i++)
            {
                if (gameworld.Stations[i].Moving)
                {
                    if (gameworld.Stations[i].AcceptButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].AcceptButton.Visible)
                        {
                            if (buyingbuilding)
                            {
                                MakeStationPurchase();
                            }
                            else
                            {
                                FinishStationPlacement();
                            }
                            i = gameworld.Stations.Count;
                        }
                    }
                    else if (gameworld.Stations[i].CancelButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].CancelButton.Visible)
                        {
                            if (buyingbuilding)
                            {
                                CancelStationPurchase();
                            }
                            else
                            {
                                UndoStationPlacement();
                            }
                            i = gameworld.Stations.Count;
                        }
                    }
                }
                else if (gameworld.Stations[i].Highlighted)
                {
                    if (gameworld.Stations[i].MoveButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].MoveButton.Visible)
                        {
                            highlightedstationid = i;
                            BeginStationPlacement();
                            i = gameworld.Stations.Count;
                        }
                    }
                    else if (gameworld.Stations[i].SellButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].SellButton.Visible)
                        {
                            highlightedstationid = i;
                            SellHighlightedStation();
                            i = gameworld.Stations.Count;
                        }
                    }
                    else if (gameworld.Stations[i].UpgradeButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].UpgradeButton.Visible)
                        {
                            highlightedstationid = i;
                            UpgradeHighlightedStation();
                            i = gameworld.Stations.Count;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A blokádokoz tartozó eltávolítás gomb eseményének kezelése. Csak akkor műkönek, ha nincsenek menük nyitva és a kurzor egy blokádon tartózkodik
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleObstacleDemolishButtonPresses(TouchLocation currenttouchlocation)
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                if (gameworld.Obstacles[i].DemolishButton.IsTapped(currenttouchlocation))
                {
                    if (gameworld.Obstacles[i].DemolishButton.Visible)
                    {
                        if (player.CanAfford(gameworld.Obstacles[i].Cost))
                        {
                            highlightedobstacleid = i;
                            DemolishHighlightedObstacle();
                            i = gameworld.Stations.Count;
                        }
                        else
                        {
                            InitializeGameMessage("Can't afford!");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A épületekhez tartozó eltávolítás gomb eseményének kezelése. Csak akkor műkönek, ha nincsenek menük nyitva és a kurzor egy épületekhez tartózkodik
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleBuildingDemolishButtonPresses(TouchLocation currenttouchlocation)
        {
            for (int i = 0; i < gameworld.Buildings.Count; i++)
            {
                if (gameworld.Buildings[i].DemolishButton.IsTapped(currenttouchlocation))
                {
                    if (gameworld.Buildings[i].DemolishButton.Visible)
                    {
                        if (player.CanAfford(gameworld.Buildings[i].Influenceamount))
                        {
                            highlightedobstacleid = i;
                            DemolishHighlightedBuilding();
                            i = gameworld.Stations.Count;
                        }
                        else
                        {
                            InitializeGameMessage("Can't afford!");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A képernyőbillentyűzet gombnyomásainak kezelése. Névmegadás végeztével a game over esemény is megkezdődik
        /// </summary>
        /// <params>A jelenlegi elsődleges érintés pozíciója: a TouchCollection [0]-dik eleme </params>
        private void HandleOnScreenKeyboardButtonPresses(TouchLocation currenttouchlocation)
        {
            onscreenkeyboard.AddPressedKey(tc);
            if (onscreenkeyboard.SubmitButton.IsTapped(currenttouchlocation) && onscreenkeyboard.SubmitButton.Visible)
            {
                ExecuteGameOver();
            }
        }

        #endregion
        #region Számolás

        /// <summary>
        /// Ha változó értéke negatív, 0-t ad vissza. Más esetben vissza adja változatlanul
        /// </summary>
        /// <param name="a">Ellenőrizendő szám</param>
        public static int RemoveOffsetMin(int a)
        {
            if (a < 0)
            {
                return 0;
            }
            return a;
        }

        /// <summary>
        /// Ha az első változó értéke meghaladja a másodikat, a másodikat adja vissza. Más esetben vissza adja az elsőt változatlanul
        /// </summary>
        /// <param name="a">Első ellenőrizendő szám</param>
        /// <param name="b">Második ellenőrizendő szám</param>
        public static int RemoveOffsetMax(int a, int b)
        {
            if (a > b)
            {
                return b;
            }
            return a;
        }

        /// <summary>
        /// Visszaadja egy négyzet vagy téglalap közepének a koordinátáit
        /// </summary>
        /// <param name="height">Az alakzat szélessége</param>
        /// <param name="width">Az alakzat magassága</param>
        public static Vector2 FindCenter(int width, int height)
        {
            return new Vector2(width / 2, height / 2);
        }

        /// <summary>
        /// Alapbeállításba helyezi a DPAD gombok ismétlődő aktiválási időközét azesetre, ha a gombot a játékos letartja
        /// </summary>
        private void InitializeDpadMoveInterval()
        {
            dpadmoveinterval = dpadmoveintervalinitial;
        }

        /// <summary>
        /// A DPAD gombok ismétlődő aktiválási időközét alapbeállításba helyezi ha annak időköze lejárt
        /// </summary>
        private void DpadMoveIntervalNextTick()
        {
            dpadmoveinterval--;
            if (dpadmoveinterval == 0)
            {
                InitializeDpadMoveInterval();
            }
        }

        #endregion
        #region Játéküzenet

        /// <summary>
        /// Bekér egy üzenetet, majd játékalkalom futása alatt kiírja a HUD-ra vörös betűkkel
        /// </summary>
        /// <param name="message">Kiírandó üzenet</param>
        private void InitializeGameMessage(string message)
        {
            gamemessage = message;
            lengthofmessageshow=3;
            messagetimer.StartTimer(lengthofmessageshow);
        }

        #endregion
        #region Idő, Kifizetés

        /// <summary>
        /// Kifizeti a játékosnak az általa összegyüjtött bevételt, majd újraindítja a hónapokat számoló időzítőt
        /// </summary>
        private void PayIncome()
        {
            if (!gamehasended && incometimer.Timeleft < 0)
            {
                incometimer.StartTimer(lengthofincomeinterval);
                player.AddMoney(gameworld.CurrentIncome);
                NextMonth();
            }
        }

        /// <summary>
        /// Eltárolja a jelenlegi évet, amit később a program használja fel mint a játékban szereplő dátum éve
        /// </summary>
        private void InitializeYear()
        {
            currentyear = DateTime.Now.Year;
        }

        /// <summary>
        /// Alapbeállításba (Januárra) helyezi a játékbeli dátum hónapját
        /// </summary>
        private void InitializeMonth()
        {
            monthcount = 1;
        }

        /// <summary>
        /// Átlépteti a játékbeli dátumot a következő hónapra. 13.hónap esetén Januárral folytatódik
        /// </summary>
        private void NextMonth()
        {
            monthcount++;
            if (monthcount > 12)
            {
                currentyear += 1;
                monthcount = 1;
            }
        }

        /// <summary>
        /// Vissza adja a játékbeli hónapot szövegként
        /// </summary>
        private string CurrentMonth
        {
            get
            {
                switch (monthcount)
                {
                    case 1: return "January";
                    case 2: return "February";
                    case 3: return "March";
                    case 4: return "April";
                    case 5: return "May";
                    case 6: return "June";
                    case 7: return "July";
                    case 8: return "August";
                    case 9: return "September";
                    case 10: return "October";
                    case 11: return "November";
                    case 12: return "December";
                    default: return "undefined";
                }
            }
        }

        #endregion
        #region Főmenű megjelenítés

        /// <summary>
        /// Megnyitja a főmenüt. Ha játékalkalom futás allatt van, kap open és close gombokat is
        /// </summary>
        private void OpenMainMenu()
        {
            menu.OpenMainMenu();
        }

        /// <summary>
        /// Megnyitja az új játékmód kiválasztás menüjét.  Ha játékalkalom futás allatt van, kap open és close gombokat is
        /// </summary>
        private void OpenPlayMenu()
        {
            menu.OpenPlayMenu();
        }

        /// <summary>
        /// Megnyitja a beállítások menüt. Ha játékalkalom futás allatt van, kap open és close gombokat is
        /// </summary>
        private void OpenOptions()
        {
            menu.OpenOptions();
        }

        /// <summary>
        /// Megnyitja a pontszámlistát. Ha játékalkalom futás allatt van, kap open és close gombokat is
        /// </summary>
        private void OpenScores()
        {
            menu.OpenScores();
        }

        /// <summary>
        /// Bezárja a menüt
        /// </summary>
        private void CloseMenu()
        {
            menu.CloseMenu();
        }

        #endregion
        #region Főmenű gombesemények

        /// <summary>
        /// Elindít egy új általános játékalkalmat. A pálya normál méretű, időzítő be van kapcsolva és az állomások felújítása engedélyezett
        /// </summary>
        private void StartRegularSession()
        {
            menu.ShowLoadScreen();
            ActivateGame();
            InitializeSession(false, false, false);
        }

        /// <summary>
        /// Elindít egy nehézmódú játékalkalmat. A pálya kis méretű, időzítő be van kapcsolva és az állomások felújítása nem engedélyezett
        /// </summary>
        private void StartHardSession()
        {
            menu.ShowLoadScreen();
            ActivateGame();
            InitializeSession(true, true, false);
        }

        /// <summary>
        /// Elindít egy sandbox játékalkalmat. A pálya normál méretű, időzítő ki van kapcsolva de az állomások felújítása engedélyezett
        /// </summary>
        private void StartSandboxSession()
        {
            menu.ShowLoadScreen();
            ActivateGame();
            InitializeSession(false, false, true);
        }

        /// <summary>
        /// Hang be és ki kapcsolására használt gombesemény. !!! Jelenleg nincs megvalósitva !!!
        /// </summary>
        private void ToggleSound()
        {
            //toggle sound
        }

        /// <summary>
        /// Az eddigi elmentett pontszámok végleges eltörlése
        /// </summary>
        private void ResetStats()
        {
            InitializeGameMessage("stats reset!");
            TruncateStatTables();
        }

        #endregion
        #region Vásárlás menü logika

        /// <summary>
        /// A vásárlás menü megjelenítése
        /// </summary>
        private void OpenBuyMenu()
        {
            if (!menu.MenuIsOpen)
            {
                buymenu.OpenBuyMenu();
            }
        }

        /// <summary>
        /// A vásárlás menü bezárása
        /// </summary>
        private void CloseBuyMenu()
        {
            buymenu.CloseBuyMenu();
        }

        /// <summary>
        /// Frissíti a hud láthatóságát attól függően, hogy nyitva van-e a vásárlás menü. Ha nyitva van, eltűnik a DPAD és a zoom gombok
        /// </summary>
        private void UpdateHudVisibilityBasedOnBuyMenu()
        {
            if (buymenu.BuymenuIsOpen)
            {
                zoomin.Visible = false;
                zoomout.Visible = false;
                dpad_up.Visible = false;
                dpad_down.Visible = false;
                dpad_left.Visible = false;
                dpad_right.Visible = false;
            }
            else
            {
                zoomin.Visible = true;
                zoomout.Visible = true;
                dpad_up.Visible = true;
                dpad_down.Visible = true;
                dpad_left.Visible = true;
                dpad_right.Visible = true;
            }
        }

        /// <summary>
        /// Frissíti a játékos szintjén attól függően, hogy hány végállomást épített. Végállomások száma = szint
        /// </summary>
        private void UpdatePlayerLevel()
        {
            player.Level = gameworld.CountTerminiOnMap();
        }

        #endregion
        #region Játékmenet

        /// <summary>
        /// Tudatja a játékmennettel és a menüvel, hogy létezik egy játékalkalom
        /// </summary>
        private void ActivateGame()
        {
            menu.StartGameSession();
            isgamesessionactive = true;
        }

        /// <summary>
        /// Tudatja a játékmennettel és a menüvel, hogy jelenleg nincs játékalkalom
        /// </summary>
        private void DeactivateGame()
        {
            isgamesessionactive = false;
        }

        /// <summary>
        /// A 2 játékbeli év letelte, és név bekérse után fut. Elmenti a pontszámot, nevet, dátumot és kilépteti a játékost a főmenübe
        /// </summary>
        private void ExecuteGameOver()
        {
            var conn = GetConnection();
            player.Name = onscreenkeyboard.CurrentText;
            InsertNewScore();
            onscreenkeyboard.CloseKeyboard();
            isgamesessionactive = false;
            OpenMainMenu();
            menu.EndGameSession();
        }

        /// <summary>
        /// A játék kilép, ha a hardveres Back gomb megnyomásra kerül
        /// </summary>
        private void ExitAppIfBackButtonIsPressed()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
        }

        #endregion
        #region SQL Kapcsolat, SQL Funkciók

        /// <summary>
        /// Vissza ad egy működő SQLite kapcsolatot a mybpt.db adatbázissal
        /// </summary>
        private static SqliteConnection GetConnection()
        {
            Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString());
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "mybpt.db");
            bool exists = File.Exists(dbPath);
            var conn = new SqliteConnection("Data Source=" + dbPath);
            return conn;
        }

        /// <summary>
        /// Betölti az összes textúranevet a mybpt.db adatbázisból
        /// </summary>
        private void LoadTexturesIntoTextureCollection()
        {
            var sql = "SELECT `Name`,`isbuilding`,`level`,`type` FROM `textures`;";

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            texturecollection.AddTexture(reader.GetString(0), Content.Load<Texture2D>(reader.GetString(0)), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Létrehoz egy új játékost a mybpt.db players táblájába
        /// </summary>
        /// <param name="playername">Az új játékos neve</param>
        private static void CreateNewPlayer(string playername)
        {

            string sql = "INSERT INTO players (Name) VALUES (@Name);";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Name", playername);

                    cmd.ExecuteScalar();
                }

                conn.Close();
            }
        }

        /// <summary>
        /// Ellenőrzi hogy a jelenlegi játékos neve már megtalálható-e a mybpt.db-ben, ha nem akkor létrehozza és elmenti azzal a pontszámot, ha igen akkor a már létező játékosnévnek menti el a pontszámot
        /// </summary>
        private void InsertNewScore()
        {
            bool playerfound = false;
            var players = GetAllPlayers();
            if (!playerfound)
            {
                CreateNewPlayer(player.Name);
                players = GetAllPlayers();
            }
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i]==player.Name)
                {
                    CreateNewScore(i+1, gameworld.CurrentIncome);
                    playerfound = true;
                    i = players.Count;
                }
            }
        }

        /// <summary>
        /// Létrehoz egy új pontszámot a mybpt.db scores táblájába
        /// </summary>
        /// <param name="playermoney">A játékos által elért teljes bevétel</param>
        /// <param name="player_id">A játékos azonosítója a players táblából (scores-nak ez egy foreign key)</param>
        private static void CreateNewScore(int player_id, int playermoney)
        {

            string sql = "INSERT INTO scores (player_id,totalmoney,timestamp) VALUES (@Player_Id,@TotalMoney,@Timestamp);";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Player_Id", player_id);
                    cmd.Parameters.AddWithValue("@TotalMoney", playermoney);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToLongDateString());
                    cmd.ExecuteScalar();
                }

                conn.Close();
            }
        }

        /// <summary>
        /// Vissza adja az összes játékos nevét a mybpt.db players táblából
        /// </summary>
        private static List<string> GetAllPlayers()
        {
            List<string> players = new List<string>();
            var sql = "SELECT * FROM players ORDER BY id ASC;";

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            players.Add(reader.GetString(1));
                    }
                }
                conn.Close();
            }
            return players;
        }

        /// <summary>
        /// Vissza adja az összes pontegyedelőfordulást a mybpt.db scores táblából
        /// </summary>
        private static List<ScoreInstance> GetAllScores()
        {
               List<ScoreInstance> scores = new List<ScoreInstance>();
               var sql = "SELECT scores.id, scores.totalmoney,players.name, scores.timestamp FROM scores INNER JOIN players ON players.id=scores.player_id ORDER BY scores.totalmoney Desc;";

                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                               scores.Add(new ScoreInstance(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3)));
                        }
                    }
                     conn.Close();
                }
                return scores;
        }

        /// <summary>
        /// Kitöröl minden adatot a mybpt.db scores és players tábláiból
        /// </summary>
        private static void TruncateStatTables()
        {
            List<ScoreInstance> scores = GetAllScores();
            for (int i = 0; i < scores.Count; i++)
            {
                string sql = "DELETE FROM scores WHERE id="+scores[i].Id;

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql; ;
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
        }
        #endregion 
    }
}
