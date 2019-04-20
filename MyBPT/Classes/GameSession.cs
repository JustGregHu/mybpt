using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


#if DEBUG
//debug function
#endif

namespace MyBPT.Classes { 
    public class GameSession : Game
    {
        //Objects for calculation
        FrameCounter frameCounter;
        IsoCalculator isoCalculator;
        Perlin perlin;

        //Graphical objects
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatchHud;
        SpriteFont font;
        GameTextures texturecollection;

        //Player objects
        Camera camera;
        Vector2 isometriccameraposition;
        Camera hud;
        TouchCollection tc;
        Player player;

        //Gameworld objects
        GameWorld gameworld;
        List<Station> stations;

        //Buttons, menus
        BuyMenu buymenu;
        Button zoomin;
        Button zoomout;
        Button dpad_up;
        Button dpad_down;
        Button dpad_left;
        Button dpad_right;

        //Variables
        int hudmargin;
        bool movingbuilding;
        bool buyingbuilding;
        int highlightedbuildingid;
        int highlightedobstacleid;
        Point preferredscreensize;
        Point tileSize;
        float viewdistance;
        int stationcost;
        int terminuscost;

        //Core functions

        public GameSession()
        {
            //Initiates graphics, loads content file (throws if a nonexistant texture is being referenced)
            preferredscreensize = new Point(1280, 720);
            graphics = new GraphicsDeviceManager(this);
            try { Content.RootDirectory = "Content"; }
            catch (Exception e){Console.WriteLine(e.StackTrace);}
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = preferredscreensize.X;
            graphics.PreferredBackBufferHeight = preferredscreensize.Y;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();
        }
        protected override void Initialize()
        {   //Display variables
            tileSize = new Point(200, 100);
            hudmargin = 50;
            viewdistance = 1.1f;
            //Objects for calculation
            frameCounter = new FrameCounter();
            isoCalculator = new IsoCalculator();
            perlin = new Perlin();

            //Logic variables
            movingbuilding = false;
            buyingbuilding = false;

            //Player variables
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            font = Content.Load<SpriteFont>("regulartext");
            camera = new Camera(GraphicsDevice.Viewport);
            hud = new Camera(GraphicsDevice.Viewport);
            player = new Player("testperson", 1000, 1);

            //Gameworld variables
            stations = new List<Station>();
            stationcost=300;
            terminuscost=1500;
            base.Initialize();
        }
        protected override void LoadContent()
        {
            //Textures, SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHud = new SpriteBatch(GraphicsDevice);
            texturecollection = new GameTextures();
            LoadTexturesIntoTextureCollection();

            //Map Generation
            gameworld = new GameWorld(texturecollection.GetTextures());
            gameworld.GenerateMap(spriteBatch, GraphicsDevice);
            gameworld.InitiateHighlightTile();
            SnapCameraToSelectedTile();

            //Buttons, Menus
            buymenu = new BuyMenu(GraphicsDevice, preferredscreensize, texturecollection);
            int zoomwidth = texturecollection.GetTextures()["zoomout"].Width;
            zoomin = new Button(new Vector2(preferredscreensize.X - (zoomwidth + hudmargin), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["zoomin"]);
            zoomout = new Button(new Vector2(preferredscreensize.X-((zoomwidth + hudmargin) * 2), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["zoomout"]);

            dpad_left = new Button(new Vector2(hudmargin-20, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["arrow_w"]);
            dpad_right = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 70 - hudmargin), texturecollection.GetTextures()["arrow_e"]);
            dpad_up = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["arrow_n"]);
            dpad_down = new Button(new Vector2(hudmargin-20, preferredscreensize.Y -70 -hudmargin), texturecollection.GetTextures()["arrow_s"]);

            //DEBUG STATIONS
            stations.Add(new Station(texturecollection, gameworld, new Point(16,5),2,"Astoria", stationcost, false,4));
            stations.Add(new Station(texturecollection, gameworld, new Point(9,5), 2, "Blaha Lujza Ter", stationcost, false, 3));
            stations.Add(new Station(texturecollection, gameworld, new Point(5, 5), 2, "Keleti Palyaudvar", terminuscost, true, 4));
        }
        protected override void UnloadContent()
        {

        }
        protected override void Update(GameTime gameTime)
        {
            UpdateTouchCollection();
            //HUD Element, Button press handlers
            CheckIfAnyStationsAreHighlighted();
            CheckIfAnyObstaclesAreHighlighted();
            UpdateHudVisibilityBasedOnBuyMenu();
            UpdatePlayerLevel();
            if (tc.Count > 0)
            {
                TouchLocation currenttouchlocation = tc[0];
                HandleZoomButtonPresses(currenttouchlocation);
                HandleDpadPresses(currenttouchlocation);
                HandleBuyMenuButtonPresses(currenttouchlocation);
                HandleStationButtonPresses(currenttouchlocation);
                HandleDemolishButtonPresses(currenttouchlocation);
            }
            // should go here : player.UpdatePlayerLevel(terminus count);

            //Updates the camera position
            camera.Update(gameTime, tc);

            //Finaly..
            ExitAppIfBackButtonIsPressed();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform);
            UpdateIsometricCamera();
            DrawMap();
            UpdateHighlightedTile();
            DrawObstacles();
            DrawStations();
            spriteBatch.End();

            spriteBatchHud.Begin(SpriteSortMode.Immediate,
                                BlendState.AlphaBlend,
                                SamplerState.PointClamp,
                                DepthStencilState.None,
                                RasterizerState.CullNone);
            DrawMainHud();
            DrawBuyMenuElements();
            DrawDemolitionMenu();
#if DEBUG
            DrawFPS(gameTime);
#endif
            spriteBatchHud.End();
            base.Draw(gameTime);
        }

        //Player, camera, touch functions
        private void LoadTexturesIntoTextureCollection()
        {
            texturecollection.AddTexture("stone", Content.Load<Texture2D>("stone"));
            texturecollection.AddTexture("snow", Content.Load<Texture2D>("snow"));
            texturecollection.AddTexture("grass", Content.Load<Texture2D>("grass"));
            texturecollection.AddTexture("water", Content.Load<Texture2D>("water"));
            texturecollection.AddTexture("sand", Content.Load<Texture2D>("sand"));
            texturecollection.AddTexture("hill", Content.Load<Texture2D>("hill"));
            texturecollection.AddTexture("snowyhill", Content.Load<Texture2D>("snowyhill"));
            texturecollection.AddTexture("station", Content.Load<Texture2D>("station"));
            texturecollection.AddTexture("terminus", Content.Load<Texture2D>("terminus"));
            texturecollection.AddTexture("highlight", Content.Load<Texture2D>("highlight"));
            texturecollection.AddTexture("highlight_lightblue", Content.Load<Texture2D>("highlight_lightblue"));
            texturecollection.AddTexture("selection_tick", Content.Load<Texture2D>("selection_tick"));
            texturecollection.AddTexture("selection_cross", Content.Load<Texture2D>("selection_cross"));
            texturecollection.AddTexture("buymenu_open", Content.Load<Texture2D>("buymenu_open"));
            texturecollection.AddTexture("buymenu_close", Content.Load<Texture2D>("buymenu_close"));
            texturecollection.AddTexture("buymenu_station", Content.Load<Texture2D>("buymenu_station"));
            texturecollection.AddTexture("buymenu_terminus", Content.Load<Texture2D>("buymenu_terminus"));
            texturecollection.AddTexture("buildingmenu_move", Content.Load<Texture2D>("buildingmenu_move"));
            texturecollection.AddTexture("buildingmenu_sell", Content.Load<Texture2D>("buildingmenu_sell"));
            texturecollection.AddTexture("buildingmenu_upgrade", Content.Load<Texture2D>("buildingmenu_upgrade"));
            texturecollection.AddTexture("demolishmenu_clear", Content.Load<Texture2D>("demolishmenu_clear"));
            texturecollection.AddTexture("arrow_n", Content.Load<Texture2D>("arrow_n"));
            texturecollection.AddTexture("arrow_e", Content.Load<Texture2D>("arrow_e"));
            texturecollection.AddTexture("arrow_w", Content.Load<Texture2D>("arrow_w"));
            texturecollection.AddTexture("arrow_s", Content.Load<Texture2D>("arrow_s"));
            texturecollection.AddTexture("zoomin", Content.Load<Texture2D>("zoomin"));
            texturecollection.AddTexture("zoomout", Content.Load<Texture2D>("zoomout"));
        }
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
            for (int i = 0; i < stations.Count; i++)
            {
                stations[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }
        private void CheckIfAnyObstaclesAreHighlighted()
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                gameworld.Obstacles[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }
        private int IdOfHighlightedStation
        {
            get
            {
                for (int i = 0; i < stations.Count; i++)
                {
                    if (stations[i].Highlighted)
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
            for (int i = 0; i < stations.Count; i++)
            {
                stations[i].DrawAffectionOfArea(spriteBatch);
            }
            for (int i = 0; i < stations.Count; i++)
            {
                stations[i].Draw(spriteBatch);
            }
        }
        private void DrawObstacles()
        {
            for (int i = 0; i < gameworld.Obstacles.Count; i++)
            {
                gameworld.Obstacles[i].Draw(spriteBatch);
            }
        }
        private void DrawZoomButtons()
        {
            zoomin.Draw(spriteBatchHud);
            zoomout.Draw(spriteBatchHud);
        }
        private void DrawDpad()
        {
            dpad_up.Draw(spriteBatchHud);
            dpad_right.Draw(spriteBatchHud);
            dpad_down.Draw(spriteBatchHud);
            dpad_left.Draw(spriteBatchHud);
        }
        private void DrawMainHud()
        {
            if (!buymenu.BuymenuIsOpen)
            {
                DrawZoomButtons();
                DrawDpad();
                int idofhighlightedstation = IdOfHighlightedStation;
                if (idofhighlightedstation > 0)
                {
                    if (stations[idofhighlightedstation].Isterminus)
                    {
                        spriteBatchHud.DrawString(font, "Terminus", new Vector2(50, 50), Color.White);
                    }
                    else
                    {
                        spriteBatchHud.DrawString(font, "Station", new Vector2(50, 50), Color.White);
                    }
                    spriteBatchHud.DrawString(font, stations[idofhighlightedstation].GetStationType(), new Vector2(50, 100), Color.White);
                    spriteBatchHud.DrawString(font, "Influence : " + stations[idofhighlightedstation].Effectradius.ToString() + " blocks", new Vector2(50, 150), Color.White);
                    spriteBatchHud.DrawString(font, stations[idofhighlightedstation].Description, new Vector2(50, 200), Color.White);
                    stations[idofhighlightedstation].DrawButtons(spriteBatchHud);
                }
                else
                {
                    spriteBatchHud.DrawString(font, player.Name + "   Lvl " + player.Level, new Vector2(50, 50), Color.White);
                    spriteBatchHud.DrawString(font, "Cash: " + player.Money, new Vector2(50, 100), Color.White);
                }
            }
        }
        private void DrawBuyMenuElements()
        {
            if (!movingbuilding)
            {
                buymenu.Draw(spriteBatchHud);
            }
        }
        private void DrawDemolitionMenu()
        {
            if (!movingbuilding && !buymenu.BuymenuIsOpen)
            {
                for (int i = 0; i < gameworld.Obstacles.Count; i++)
                {
                    gameworld.Obstacles[i].DrawButtons(spriteBatchHud);
                }
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
            stations[highlightedbuildingid].InitiateMove();
        }
        private void MarkPlacementForPurchase(bool isterminus)
        {
            highlightedbuildingid = stations.Count;
            if (isterminus)
            {
                stations.Add(new Station(texturecollection, gameworld, new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y), 2, "new terminus!", 750, true, 4));
            }
            else
            {
                stations.Add(new Station(texturecollection, gameworld, new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y), 2, "new station!", 250, false, 4));

            }
            BeginBuildingPlacement();
            buyingbuilding = true;
        }
        private void FinishBuildingPlacement()
        {
            if (!gameworld.IsThereanObstacleAt(stations[highlightedbuildingid].Coordinates) && !gameworld.IsThereWaterAt(stations[highlightedbuildingid].Coordinates))
            {
                stations[highlightedbuildingid].Highlighted = false;
                stations[highlightedbuildingid].FinalizeMove(gameworld);
                movingbuilding = false;
                buyingbuilding = false;
            }
        }
        private void MakeBuildingPurchase()
        {
            if (!gameworld.IsThereanObstacleAt(stations[highlightedbuildingid].Coordinates) && !gameworld.IsThereWaterAt(stations[highlightedbuildingid].Coordinates))
            {
                player.AddMoney(-stations[highlightedbuildingid].Cost);
                FinishBuildingPlacement();
                buyingbuilding = false;
            }

        }
        private void UndoBuildingPlacement()
        {
            stations[highlightedbuildingid].Highlighted = false;
            stations[highlightedbuildingid].RollbackMove();
            movingbuilding = false;
            buyingbuilding = false;
        }
        private void CancelBuildingPurchase()
        {
            UndoBuildingPlacement();
            stations.RemoveAt(highlightedbuildingid);
        }
        private void SellHighlightedBuilding()
        {
            player.AddMoney(stations[highlightedbuildingid].SellPrice);
            stations.RemoveAt(highlightedbuildingid);
            highlightedbuildingid = -1;
        }
        private void DemolishHighlightedObstacle()
        {
            player.AddMoney(-gameworld.Obstacles[highlightedobstacleid].Cost);
            gameworld.Obstacles.RemoveAt(highlightedobstacleid);
            highlightedobstacleid = -1;
        }

        //Building movement
        private void MoveBuildingUp()
        {
            stations[highlightedbuildingid].MoveStationTo(new Point(stations[highlightedbuildingid].Coordinates.X - 1, stations[highlightedbuildingid].Coordinates.Y),gameworld);
        }
        private void MoveBuildingDown()
        {
            stations[highlightedbuildingid].MoveStationTo(new Point(stations[highlightedbuildingid].Coordinates.X + 1, stations[highlightedbuildingid].Coordinates.Y ), gameworld);
        }
        private void MoveBuildingLeft()
        {
            stations[highlightedbuildingid].MoveStationTo(new Point(stations[highlightedbuildingid].Coordinates.X, stations[highlightedbuildingid].Coordinates.Y - 1), gameworld);
        }
        private void MoveBuildingRight()
        {
            stations[highlightedbuildingid].MoveStationTo(new Point(stations[highlightedbuildingid].Coordinates.X, stations[highlightedbuildingid].Coordinates.Y + 1), gameworld);
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
            if (dpad_up.IsTapped(currenttouchlocation) && dpad_up.Visible)
            {
                DPAD_Up();
                if (movingbuilding)
                {
                    MoveBuildingUp();
                }
            }
            if (dpad_down.IsTapped(currenttouchlocation) && dpad_down.Visible)
            {
                DPAD_Down();
                if (movingbuilding)
                {
                    MoveBuildingDown();
                }
            }
            if (dpad_left.IsTapped(currenttouchlocation) && dpad_left.Visible)
            {
                DPAD_Left();
                if (movingbuilding)
                {
                    MoveBuildingLeft();
                }
            }
            if (dpad_right.IsTapped(currenttouchlocation) && dpad_right.Visible)
            {
                DPAD_Right();
                if (movingbuilding)
                {
                    MoveBuildingRight();
                }
            }
        }
        private void HandleBuyMenuButtonPresses(TouchLocation currenttouchlocation)
        {
            if (buymenu.OpenButton.IsTapped(currenttouchlocation) && buymenu.OpenButton.Visible)
            {
                currenttouchlocation = new TouchLocation();
                OpenBuyMenu();
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
                        CloseBuyMenu();
                        SnapCameraToSelectedTile();
                        MarkPlacementForPurchase(true);
                    }
                    else
                    {
                        //MESSAGE: CANT AFFORD TERMINUS!
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
                        //MESSAGE: CANT AFFORD STATION!
                    }
                }
            }
        }
        private void HandleStationButtonPresses(TouchLocation currenttouchlocation)
        {
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i].Moving)
                {
                    if (stations[i].AcceptButton.IsTapped(currenttouchlocation))
                    {
                        if (stations[i].AcceptButton.Visible)
                        {
                            if (buyingbuilding)
                            {
                                MakeBuildingPurchase();
                            }
                            else
                            {
                                FinishBuildingPlacement();
                            }
                            i = stations.Count;
                        }
                    }
                    else if (stations[i].CancelButton.IsTapped(currenttouchlocation))
                    {
                        if (stations[i].CancelButton.Visible)
                        {
                            if (buyingbuilding)
                            {
                                CancelBuildingPurchase();
                            }
                            else
                            {
                                UndoBuildingPlacement();
                            }
                            i = stations.Count;
                        }
                    }
                }
                else if (stations[i].Highlighted)
                {
                    if (stations[i].MoveButton.IsTapped(currenttouchlocation))
                    {
                        if (stations[i].MoveButton.Visible)
                        {
                            highlightedbuildingid = i;
                            BeginBuildingPlacement();
                            i = stations.Count;
                        }
                    }
                    else if (stations[i].SellButton.IsTapped(currenttouchlocation))
                    {
                        if (stations[i].SellButton.Visible)
                        {
                            highlightedbuildingid = i;
                            SellHighlightedBuilding();
                            i = stations.Count;
                        }
                    }
                }
            }
        }
        private void HandleDemolishButtonPresses(TouchLocation currenttouchlocation)
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
                            i = stations.Count;
                        }
                        else
                        {
                            //MESSAGE: CANT AFFORD STATION!
                        }
                    }
                }
            }
        }

        //Location, maths, renderdistance functions
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

        //Buymenu logic functions
        private void OpenBuyMenu()
        {
            buymenu.OpenBuyMenu();
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
        private int CountTerminiOnMap()
        {
            int count = 0;
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i].Isterminus)
                {
                    count++;
                }
            }
            return count;
        }
        private void UpdatePlayerLevel()
        {
            player.Level = CountTerminiOnMap();
        }


        //Misc.
        private void ExitAppIfBackButtonIsPressed()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
        }
    }
}
