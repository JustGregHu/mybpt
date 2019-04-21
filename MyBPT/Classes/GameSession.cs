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
        int dpadmoveinterval;
        int dpadmoveintervalinitial;

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
        int highlightedstationid;
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
            viewdistance = 2.2f;
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
            player = new Player("testperson", 2000, 1);
            dpadmoveintervalinitial = 10;
            dpadmoveinterval = dpadmoveintervalinitial;

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
            DrawRoads();
            UpdateHighlightedTile();
            DrawObstacles();
            DrawStations();
            DrawBuildings();
            spriteBatch.End();

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
            texturecollection.AddTexture("roadright", Content.Load<Texture2D>("roadright"));
            texturecollection.AddTexture("roadleft", Content.Load<Texture2D>("roadleft"));
            texturecollection.AddTexture("roadcross", Content.Load<Texture2D>("roadcross"));
            texturecollection.AddTexture("bridgeright", Content.Load<Texture2D>("bridgeright"));
            texturecollection.AddTexture("bridgeleft", Content.Load<Texture2D>("bridgeleft"));
            texturecollection.AddTexture("station", Content.Load<Texture2D>("station"));
            texturecollection.AddTexture("terminus", Content.Load<Texture2D>("terminus"));
            texturecollection.AddTexture("highlight", Content.Load<Texture2D>("highlight"));
            texturecollection.AddTexture("highlight_lightblue", Content.Load<Texture2D>("highlight_lightblue"));
            texturecollection.AddTexture("selection_tick", Content.Load<Texture2D>("selection_tick"));
            texturecollection.AddTexture("selection_cross", Content.Load<Texture2D>("selection_cross"));
            texturecollection.AddTexture("buymenu_open", Content.Load<Texture2D>("buymenu_open"));
            texturecollection.AddTexture("building_residential1", Content.Load<Texture2D>("building_residential1"));
            texturecollection.AddTexture("building_residential2", Content.Load<Texture2D>("building_residential2"));
            texturecollection.AddTexture("building_commercial1", Content.Load<Texture2D>("building_commercial1"));
            texturecollection.AddTexture("building_commercial2", Content.Load<Texture2D>("building_commercial2"));
            texturecollection.AddTexture("building_industrial1", Content.Load<Texture2D>("building_industrial1"));
            texturecollection.AddTexture("building_industrial2", Content.Load<Texture2D>("building_industrial2"));
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
        private void DrawObstacleDemolitionMenu()
        {
            if (!movingbuilding && !buymenu.BuymenuIsOpen)
            {
                for (int i = 0; i < gameworld.Obstacles.Count; i++)
                {
                    gameworld.Obstacles[i].DrawButtons(spriteBatchHud);
                }
            }

        }
        private void DrawBuildingDemolitionMenu()
        {
            if (!movingbuilding && !buymenu.BuymenuIsOpen)
            {
                for (int i = 0; i < gameworld.Buildings.Count; i++)
                {
                    gameworld.Buildings[i].DrawButtons(spriteBatchHud);
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
            stations[highlightedstationid].InitiateMove();
        }
        private void MarkPlacementForPurchase(bool isterminus)
        {
            highlightedstationid = stations.Count;
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
        if (!gameworld.IsThereanObstacleAt(stations[highlightedstationid].Coordinates) && !gameworld.IsThereWaterAt(stations[highlightedstationid].Coordinates))
            if (!gameworld.IsThereARoadAt(stations[highlightedstationid].Coordinates) &&gameworld.IsThereARoadNextTo(stations[highlightedstationid].Coordinates,1))
            {
                    if (!gameworld.IsThereABuildingAt(stations[highlightedstationid].Coordinates))
                    {
                        stations[highlightedstationid].Highlighted = false;
                        stations[highlightedstationid].FinalizeMove(gameworld);
                        movingbuilding = false;
                        buyingbuilding = false;
                    }
            }
        }
        private void MakeBuildingPurchase()
        {
            if (!gameworld.IsThereanObstacleAt(stations[highlightedstationid].Coordinates) && !gameworld.IsThereWaterAt(stations[highlightedstationid].Coordinates))
            {
                player.AddMoney(-stations[highlightedstationid].Cost);
                FinishBuildingPlacement();
                buyingbuilding = false;
            }

        }
        private void UndoBuildingPlacement()
        {
            stations[highlightedstationid].Highlighted = false;
            stations[highlightedstationid].RollbackMove();
            movingbuilding = false;
            buyingbuilding = false;
        }
        private void CancelBuildingPurchase()
        {
            UndoBuildingPlacement();
            stations.RemoveAt(highlightedstationid);
        }
        private void SellHighlightedBuilding()
        {
            player.AddMoney(stations[highlightedstationid].SellPrice);
            stations.RemoveAt(highlightedstationid);
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
            highlightedobstacleid = -1;
        }

        //Building movement
        private void MoveBuildingUp()
        {
            stations[highlightedstationid].MoveStationTo(new Point(stations[highlightedstationid].Coordinates.X - 1, stations[highlightedstationid].Coordinates.Y),gameworld);
        }
        private void MoveBuildingDown()
        {
            stations[highlightedstationid].MoveStationTo(new Point(stations[highlightedstationid].Coordinates.X + 1, stations[highlightedstationid].Coordinates.Y ), gameworld);
        }
        private void MoveBuildingLeft()
        {
            stations[highlightedstationid].MoveStationTo(new Point(stations[highlightedstationid].Coordinates.X, stations[highlightedstationid].Coordinates.Y - 1), gameworld);
        }
        private void MoveBuildingRight()
        {
            stations[highlightedstationid].MoveStationTo(new Point(stations[highlightedstationid].Coordinates.X, stations[highlightedstationid].Coordinates.Y + 1), gameworld);
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
                            highlightedstationid = i;
                            BeginBuildingPlacement();
                            i = stations.Count;
                        }
                    }
                    else if (stations[i].SellButton.IsTapped(currenttouchlocation))
                    {
                        if (stations[i].SellButton.Visible)
                        {
                            highlightedstationid = i;
                            SellHighlightedBuilding();
                            i = stations.Count;
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
