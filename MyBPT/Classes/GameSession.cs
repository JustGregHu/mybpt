using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Android.Content;
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
    public class GameSession : Game
    {
        //Objects for calculation
        FrameCounter frameCounter;
        IsoCalculator isoCalculator;
        Perlin perlin;
        CountDown incometimer;
        CountDown gametimer;
        CountDown messagetimer;

        //Graphical objects
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatchHud;
        SpriteBatch spriteBatchMenu;
        SpriteBatch spriteBatchKeyboard;
        SpriteFont font;
        GameTextures texturecollection;

        //Player objects
        Camera camera;
        Vector2 isometriccameraposition;
        Camera hud;
        TouchCollection tc;
        Player player;
        int dpadmoveinterval;
        int dpadmoveintervalinitial;

        //Gameworld objects
        GameWorld gameworld;
        bool hardmode;
        bool sandbox;

        //Buttons, menus
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

        //Variables
        bool isgamesessionactive;
        int hudmargin;
        bool movingbuilding;
        bool buyingbuilding;
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

        //Core functions
        public GameSession()
        {
            //Initiates graphics, loads content file (throws if a nonexistant texture is being referenced)
            preferredscreensize = new Point(1280, 720);
            graphics = new GraphicsDeviceManager(this);
            try { Content.RootDirectory = "Content"; }
            catch (Exception){}
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = preferredscreensize.X;
            graphics.PreferredBackBufferHeight = preferredscreensize.Y;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();
        }
        protected override void Initialize()
        {   //Display variables
            DeactivateGame();
            tileSize = new Point(200, 100);
            hudmargin = 50;
            viewdistance = 2.2f;
            //Objects for calculation
            frameCounter = new FrameCounter();
            isoCalculator = new IsoCalculator();
            perlin = new Perlin();
            incometimer = new CountDown();
            gametimer = new CountDown();
            messagetimer=new CountDown();
            gamemessage = "";
            messagetimer.StartTimer(1);

            //Player variables
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            font = Content.Load<SpriteFont>("regulartext");
            camera = new Camera(GraphicsDevice.Viewport);
            hud = new Camera(GraphicsDevice.Viewport);
            dpadmoveintervalinitial = 10;
            dpadmoveinterval = dpadmoveintervalinitial;

            //Gameworld variables
            stationcost=350;
            terminuscost=1000;

            base.Initialize();
        }
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
            menu = new Menu(GraphicsDevice, preferredscreensize, texturecollection);
            buymenu = new BuyMenu(GraphicsDevice, preferredscreensize, texturecollection);
            int zoomwidth = texturecollection.GetTextures()["hud_zoom_in"].Width;
            zoomin = new Button(new Vector2(preferredscreensize.X - (zoomwidth + hudmargin), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["hud_zoom_in"]);
            zoomout = new Button(new Vector2(preferredscreensize.X-((zoomwidth + hudmargin) * 2), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["hud_zoom_out"]);

            dpad_left = new Button(new Vector2(hudmargin-20, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["hud_dpad_west"]);
            dpad_right = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 70 - hudmargin), texturecollection.GetTextures()["hud_dpad_east"]);
            dpad_up = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["hud_dpad_north"]);
            dpad_down = new Button(new Vector2(hudmargin-20, preferredscreensize.Y -70 -hudmargin), texturecollection.GetTextures()["hud_dpad_south"]);

            onscreenkeyboard = new OnScreenKeyboard(GraphicsDevice, preferredscreensize, texturecollection);
        }
        private void InitializeSession(bool isupgradable, bool size,bool issandbox)
        {
            menu.CloseMenu();
            //Logic variables
            movingbuilding = false;
            buyingbuilding = false;

            //Player variables
            player = new Player("testperson", 2000, 1);

            //Map Generation
            gameworld = new GameWorld(texturecollection.GetTextures(),size);
            hardmode = isupgradable;
            sandbox = issandbox;
            gameworld.GenerateMap(spriteBatch, GraphicsDevice);
            gameworld.InitiateHighlightTile();
            SnapCameraToSelectedTile();

            //Gameworld variables
            InitializeYear();
            InitializeMonth();
            if (!issandbox)
            {
                lengthofgametime = 5;
                gametimer.StartTimer(lengthofgametime);
            }
            lengthofincomeinterval = 30;
            incometimer.StartTimer(lengthofincomeinterval);
        }
        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            if (isgamesessionactive && !sandbox && gametimer.Timeleft<1)
            {
                CloseMenu();
                CloseBuyMenu();
                onscreenkeyboard.OpenKeyboard();
                InitializeGameMessage("GAME OVER");
            }
            UpdateTouchCollection();
            if (isgamesessionactive)
            {
                PayIncome();
                //HUD Element, Button press handlers
                CheckIfAnyStationsAreHighlighted();
                CheckIfAnyObstaclesAreHighlighted();
                CheckIfAnyBuildingsAreHighlighted();
                UpdateHudVisibilityBasedOnBuyMenu();
                UpdatePlayerLevel();
                if (tc.Count > 0)
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
            if (tc.Count > 0)
            {
                TouchLocation currenttouchlocation = tc[0];
                HandleMenuButtonPresses(currenttouchlocation);
                HandleOnScreenKeyboardButtonPresses(currenttouchlocation);
            }

            //Updates the camera position
            camera.Update(gameTime, tc);

            //Finaly..
            ExitAppIfBackButtonIsPressed();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
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
#if DEBUG
                DrawFPS(gameTime);
#endif
                DrawGameMessage();
                spriteBatchHud.End();

            }

            spriteBatchMenu.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        RasterizerState.CullNone);

            DrawMenuElements();
            spriteBatchMenu.End();


            spriteBatchKeyboard.Begin(SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

            DrawOnScreenKeyboard();
            spriteBatchKeyboard.End();

            base.Draw(gameTime);
        }

        //Player, camera, touch functions
        private void ReInitiateTouchCollection()
        {
            tc = new TouchCollection();
            tc = TouchPanel.GetState();
        }
        private void UpdateTouchCollection()
        {
            tc = TouchPanel.GetState();
        }
        private void SnapCameraToSelectedTile()
        {
            Tile currenttile = gameworld.MapData[gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y];
            float mapx = currenttile.Position.X-preferredscreensize.X/2+ currenttile.Texture.Width/2;
            float mapy = currenttile.Position.Y- preferredscreensize.Y / 2 + currenttile.Texture.Height / 2; ;
            camera.TargetPosition = (new Vector2(mapx, mapy));
        }
        private void UpdateIsometricCamera()
        {
            isometriccameraposition = isoCalculator.IsoTo2D(new Vector2(-camera.Position.X * 2, camera.Position.Y));
        }

        //Highlighted tile related functions
        private void UpdateHighlightedTile()
        {
            gameworld.HighlightCurrentTile(spriteBatch);
        }
        private void CheckIfAnyStationsAreHighlighted()
        {
            for (int i = 0; i < gameworld.Stations.Count; i++)
            {
                gameworld.Stations[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }
        private void CheckIfAnyBuildingsAreHighlighted()
        {
            for (int i = 0; i < gameworld.Buildings.Count; i++)
            {
                gameworld.Buildings[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }
        private void CheckIfAnyObstaclesAreHighlighted()
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                gameworld.Obstacles[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }
        private void CheckIfAnyRoadsAreHighlighted()
        {
            for (int i = 0; i < gameworld.Roads.Count; i++)
            {
                gameworld.IsThereARoadAt(gameworld.CurrentTilePosition);
            }
        }
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

        //Draw functions
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
        private void DrawBuildings()
        {
            for (int i = 0; i < gameworld.Buildings.Count; i++)
            {
                gameworld.Buildings[i].Draw(spriteBatch);
            }
        }
        private void DrawObstacles()
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                gameworld.Obstacles[i].Draw(spriteBatch);
            }
        }
        private void DrawRoads()
        {
            for (int i = 0; i < gameworld.Roads.Count; i++)
            {
                gameworld.Roads[i].Draw(spriteBatch);
            }
        }
        private void DrawZoomButtons()
        {
            if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                zoomin.Draw(spriteBatchHud);
                zoomout.Draw(spriteBatchHud);
            }
        }
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
        private void DrawMainHud()
        {
            if (!buymenu.BuymenuIsOpen && !menu.MenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                DrawZoomButtons();
                DrawDpad();
                int idofhighlightedstation = IdOfHighlightedStation;
                if (idofhighlightedstation >-1)
                {
                    gameworld.Stations[idofhighlightedstation].DrawButtons(spriteBatchHud);
                    gameworld.Stations[idofhighlightedstation].DrawInfo(spriteBatchHud,font);
                }
                else
                {
                    spriteBatchHud.DrawString(font, "Lvl " + player.Level, new Vector2(50, 50), Color.White);
                    spriteBatchHud.DrawString(font, "Cash: " + player.Money, new Vector2(50, 100), Color.White);
                    spriteBatchHud.DrawString(font, "Next month: " + incometimer.Timeleft + "s", new Vector2(50, 200), Color.White);
                    spriteBatchHud.DrawString(font, currentyear +" "+CurrentMonth, new Vector2(50, 250), Color.White);
                }
            }
        }
        private void DrawMenuElements()
        {
            if (!buymenu.BuymenuIsOpen && !onscreenkeyboard.KeyboardIsOpen)
            {
                menu.Draw(spriteBatchMenu);
                if (menu.ScoresAreVisible)
                {
                    List<ScoreInstance> scores = GetAllScores();
                    List<String> scorestrings = new List<string>();
                    for (int i = 0; i < scores.Count; i++)
                    {
                        scorestrings.Add(scores[i].Playername + ": " + scores[i].Amount);
                    }
                    menu.DrawScores(spriteBatchMenu, font, scorestrings);
                }
                
            }
        }
        private void DrawBuyMenuElements()
        {
            if (!movingbuilding && (!menu.MenuIsOpen) && !onscreenkeyboard.KeyboardIsOpen)
            {
                buymenu.Draw(spriteBatchHud);
            }
        }
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
        private void DrawOnScreenKeyboard()
        {
            onscreenkeyboard.DrawOnScreenKeyboard(spriteBatchKeyboard, font);
        }
        private void DrawGameMessage()
        {
            if (messagetimer.Timeleft>0)
            {
                spriteBatchHud.DrawString(font, gamemessage, new Vector2(preferredscreensize.X - preferredscreensize.X / 2, preferredscreensize.Y - preferredscreensize.X / 2), Color.Red);
            }
        }
        private void DrawFPS(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);
            spriteBatchHud.DrawString(font, fps, new Vector2(0, 0), Color.White);
        }

        //Building placement and purchase logic functions
        private void BeginBuildingPlacement()
        {
            movingbuilding = true;
            gameworld.Stations[highlightedstationid].InitiateMove();
            gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
        }
        private void MarkPlacementForPurchase(bool isterminus)
        {
            highlightedstationid = gameworld.Stations.Count;
            if (isterminus)
            {
                gameworld.Stations.Add(new Station(GraphicsDevice, preferredscreensize, texturecollection, gameworld, new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y), true));
            }
            else
            {
                gameworld.Stations.Add(new Station(GraphicsDevice, preferredscreensize, texturecollection, gameworld, new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y), false));

            }
            BeginBuildingPlacement();
            buyingbuilding = true;
        }
        private void FinishBuildingPlacement()
        {
        if (!gameworld.IsThereanObstacleAt(gameworld.Stations[highlightedstationid].Coordinates) && !gameworld.IsThereWaterAt(gameworld.Stations[highlightedstationid].Coordinates))
            if (!gameworld.IsThereARoadAt(gameworld.Stations[highlightedstationid].Coordinates) &&gameworld.IsThereARoadNextTo(gameworld.Stations[highlightedstationid].Coordinates,1))
            {
                    if (!gameworld.IsThereABuildingAt(gameworld.Stations[highlightedstationid].Coordinates))
                    {
                        gameworld.Stations[highlightedstationid].Highlighted = false;
                        gameworld.Stations[highlightedstationid].FinalizeMove(gameworld);
                        movingbuilding = false;
                        buyingbuilding = false;
                    }
            }
        }
        private void MakeBuildingPurchase()
        {
            if (!gameworld.IsThereanObstacleAt(gameworld.Stations[highlightedstationid].Coordinates) && !gameworld.IsThereWaterAt(gameworld.Stations[highlightedstationid].Coordinates)) { 
                if (!gameworld.IsThereARoadAt(gameworld.Stations[highlightedstationid].Coordinates) && gameworld.IsThereARoadNextTo(gameworld.Stations[highlightedstationid].Coordinates, 1))
                {
                    player.AddMoney(-gameworld.Stations[highlightedstationid].Cost);
                    FinishBuildingPlacement();
                    buyingbuilding = false;
                }
        }
        

        }
        private void UndoBuildingPlacement()
        {
            gameworld.Stations[highlightedstationid].Highlighted = false;
            gameworld.Stations[highlightedstationid].RollbackMove();
            movingbuilding = false;
            buyingbuilding = false;
        }
        private void CancelBuildingPurchase()
        {
            UndoBuildingPlacement();
            gameworld.Stations.RemoveAt(highlightedstationid);
        }
        private void SellHighlightedBuilding()
        {
            player.AddMoney(gameworld.Stations[highlightedstationid].SellPrice);
            gameworld.Stations.RemoveAt(highlightedstationid);
            highlightedstationid = -1;
        }
        private void DemolishHighlightedObstacle()
        {
            player.AddMoney(-gameworld.Obstacles[highlightedobstacleid].Cost);
            gameworld.Obstacles.RemoveAt(highlightedobstacleid);
            highlightedobstacleid = -1;
        }
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
        private void UpgradeHighlightedBuilding()
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

        //Building movement
        private void MoveBuildingUp()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X - 1, gameworld.Stations[highlightedstationid].Coordinates.Y)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X - 1, gameworld.Stations[highlightedstationid].Coordinates.Y), gameworld);
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
            }

        }
        private void MoveBuildingDown()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X + 1, gameworld.Stations[highlightedstationid].Coordinates.Y)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X + 1, gameworld.Stations[highlightedstationid].Coordinates.Y ), gameworld);
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
            }
        }
        private void MoveBuildingLeft()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y - 1)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                 gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y - 1), gameworld);;
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
            }
        }
        private void MoveBuildingRight()
        {
            if (!gameworld.IsThereAStationAt(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y + 1)))
            {
                gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
                gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y + 1), gameworld);
            }
            else
            {
                gameworld.CurrentTilePosition = gameworld.Stations[highlightedstationid].Coordinates;
            }
        }

        //Button click events
        private void ZoomIn()
        {
            camera.Zoom = 1f;
            viewdistance = 1.1f;
        }
        private void ZoomOut()
        {
            camera.Zoom = 0.5f;
            viewdistance = 2.2f;
        }

        private void DPAD_Up()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X - 1, gameworld.CurrentTilePosition.Y));
            SnapCameraToSelectedTile();
        }
        private void DPAD_Down()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X + 1, gameworld.CurrentTilePosition.Y));
            SnapCameraToSelectedTile();
        }
        private void DPAD_Left()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y - 1));
            SnapCameraToSelectedTile();
        }
        private void DPAD_Right()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y + 1));
            SnapCameraToSelectedTile();
        }

        private void HandleZoomButtonPresses(TouchLocation currenttouchlocation)
        {
            if (zoomin.IsTapped(currenttouchlocation) && zoomin.Visible)
            {
                ZoomIn();
            }
            if (zoomout.IsTapped(currenttouchlocation) && zoomout.Visible)
            {
                ZoomOut();
            }
        }
        private void HandleDpadPresses(TouchLocation currenttouchlocation)
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
                            MoveBuildingUp();
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
                            MoveBuildingDown();
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
                            MoveBuildingLeft();
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
                            MoveBuildingRight();
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
                                MakeBuildingPurchase();
                            }
                            else
                            {
                                FinishBuildingPlacement();
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
                                CancelBuildingPurchase();
                            }
                            else
                            {
                                UndoBuildingPlacement();
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
                            BeginBuildingPlacement();
                            i = gameworld.Stations.Count;
                        }
                    }
                    else if (gameworld.Stations[i].SellButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].SellButton.Visible)
                        {
                            highlightedstationid = i;
                            SellHighlightedBuilding();
                            i = gameworld.Stations.Count;
                        }
                    }
                    else if (gameworld.Stations[i].UpgradeButton.IsTapped(currenttouchlocation))
                    {
                        if (gameworld.Stations[i].UpgradeButton.Visible)
                        {
                            highlightedstationid = i;
                            UpgradeHighlightedBuilding();
                            i = gameworld.Stations.Count;
                        }
                    }
                }
            }
        }
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

        private void HandleOnScreenKeyboardButtonPresses(TouchLocation currenttouchlocation)
        {
            onscreenkeyboard.AddPressedKey(tc);
            if (onscreenkeyboard.SubmitButton.IsTapped(currenttouchlocation) && onscreenkeyboard.SubmitButton.Visible)
            {
                var conn = GetConnection();
                player.Name = onscreenkeyboard.CurrentText;
                InsertNewScore();
                onscreenkeyboard.CloseKeyboard();
                isgamesessionactive = false;
                OpenMainMenu();
                menu.EndGameSession();
            }
        }

        //Location, maths, renderdistance, time functions
        private int RemoveOffsetMin(int a)
        {
            if (a < 0)
            {
                return 0;
            }
            return a;
        }
        private int RemoveOffsetMax(int a, int worldsize)
        {
            if (a > worldsize)
            {
                return worldsize;
            }
            return a;
        }
        private Vector2 FindCenter(int width, int height)
        {
            return new Vector2(width / 2, height / 2);
        }
        private void InitializeDpadMoveInterval()
        {
            dpadmoveinterval = dpadmoveintervalinitial;
        }
        private void DpadMoveIntervalNextTick()
        {
            dpadmoveinterval--;
            if (dpadmoveinterval == 0)
            {
                InitializeDpadMoveInterval();
            }
        }
        private void PayIncome()
        {
            if (incometimer.Timeleft < 0)
            {
                incometimer.StartTimer(lengthofincomeinterval);
                player.AddMoney(gameworld.CurrentIncome);
                NextMonth();
            }
        }
        private void InitializeYear()
        {
            currentyear=DateTime.Now.Year;
        }
        private void InitializeMonth()
        {
            monthcount = 1;
        }
        private void NextMonth()
        {
            monthcount++;
            if (monthcount>12)
            {
                currentyear += 1;
                monthcount = 1;
            }
        }
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
                    default:return "undefined";   
                }
            }
        }
        private void InitializeGameMessage(string message)
        {
            gamemessage = message;
            lengthofmessageshow=3;
            messagetimer.StartTimer(lengthofmessageshow);
        }

        //Menu logic functions
        private void OpenMainMenu()
        {
            menu.OpenMainMenu();
        }
        private void OpenPlayMenu()
        {
            menu.OpenPlayMenu();
        }
        private void StartRegularSession()
        {
            ActivateGame();
            InitializeSession(false,false,false);
        }
        private void StartHardSession()
        {
            ActivateGame();
            InitializeSession(true, true, false);
        }
        private void StartSandboxSession()
        {
            ActivateGame();
            InitializeSession(false, false,true);
        }
        private void OpenOptions()
        {
            menu.OpenOptions();
        }
        private void ToggleSound()
        {
            //toggle sound
        }
        private void ResetStats()
        {
            InitializeGameMessage("stats reset!");
            TruncateStatTables();
        }
        private void OpenScores()
        {
            menu.OpenScores();
        }
        private void CloseMenu()
        {
            menu.CloseMenu();
        }

        //Buymenu logic functions
        private void OpenBuyMenu()
        {
            if (!menu.MenuIsOpen)
            {
                buymenu.OpenBuyMenu();
            }
        }
        private void CloseBuyMenu()
        {
            buymenu.CloseBuyMenu();
        }
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
        private void UpdatePlayerLevel()
        {
            player.Level = gameworld.CountTerminiOnMap();
        }

        //Session functions
        private void ActivateGame()
        {
            menu.StartGameSession();
            isgamesessionactive = true;
        }
        private void DeactivateGame()
        {
            isgamesessionactive = false;
        }
        private void ExitAppIfBackButtonIsPressed()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
        }

        //SQL Connections
        private static SqliteConnection GetConnection()
        {
            Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString());
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "mybpt.db");
            bool exists = File.Exists(dbPath);
            var conn = new SqliteConnection("Data Source=" + dbPath);
            return conn;
        }
        private void LoadTexturesIntoTextureCollection()
        {
            var sql = "SELECT `Name` FROM `textures`;";

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            texturecollection.AddTexture(reader.GetString(0), Content.Load<Texture2D>(reader.GetString(0)));
                    }
                }
                conn.Close();
            }
        }
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
                    CreateNewScore(i+1, player.Money);
                    playerfound = true;
                    i = players.Count;
                }
            }
        }

        private static void CreateNewScore(int player_id, int playermoney)
        {

            string sql = "INSERT INTO scores (player_id,totalmoney) VALUES (@Player_Id,@TotalMoney);";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Player_Id", player_id);
                    cmd.Parameters.AddWithValue("@TotalMoney", playermoney);
                    cmd.ExecuteScalar();
                }

                conn.Close();
            }
        }

        private static List<string> GetAllPlayers()
        {
            List<string> scores = new List<string>();
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
                            scores.Add(reader.GetString(1));
                    }
                }
                conn.Close();
            }
            return scores;
        }

        private static List<ScoreInstance> GetAllScores()
        {
                List<ScoreInstance> scores = new List<ScoreInstance>();
               var sql = "SELECT scores.totalmoney,players.name FROM scores INNER JOIN players ON players.id=scores.player_id ORDER BY scores.totalmoney Desc LIMIT 10;";

                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                               scores.Add(new ScoreInstance(reader.GetInt32(0),reader.GetString(1)));
                        }
                    }
                     conn.Close();
                }
                return scores;
        }

        private static void TruncateStatTables()
        {
            //rewrite. delete from all
            string sql = "DELETE FROM scores;DELETE FROM  players;";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;;
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }
    }
}
