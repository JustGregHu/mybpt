using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using SQLite;
using SQLiteConnectionBuddy;
using Mono.Data.Sqlite;
using SQLiteNetExtensions.Extensions;


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
        CountDown incometimer;

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
        int incomeinterval;

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
            incometimer = new CountDown();

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
            stationcost=300;
            terminuscost=1500;
            incomeinterval = 30;
            incometimer.StartTimer(incomeinterval);
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
            int zoomwidth = texturecollection.GetTextures()["hud_zoom_in"].Width;
            zoomin = new Button(new Vector2(preferredscreensize.X - (zoomwidth + hudmargin), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["hud_zoom_in"]);
            zoomout = new Button(new Vector2(preferredscreensize.X-((zoomwidth + hudmargin) * 2), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["hud_zoom_out"]);

            dpad_left = new Button(new Vector2(hudmargin-20, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["hud_dpad_west"]);
            dpad_right = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 70 - hudmargin), texturecollection.GetTextures()["hud_dpad_east"]);
            dpad_up = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["hud_dpad_north"]);
            dpad_down = new Button(new Vector2(hudmargin-20, preferredscreensize.Y -70 -hudmargin), texturecollection.GetTextures()["hud_dpad_south"]);
        }
        protected override void UnloadContent()
        {

        }
        protected override void Update(GameTime gameTime)
        {
            PayIncome();
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



        private static SqliteConnection GetConnection()
        {
            Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString());
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "mybpt.db");
            bool exists = File.Exists(dbPath);


            var conn = new SqliteConnection("Data Source=" + dbPath);



            return conn;
        }

        private static void CreateDatabase(SqliteConnection connection)
        {
            var sql = "CREATE TABLE textures (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name ntext);";

            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }

            // Create a sample note to get the user started
            sql = "INSERT INTO textures (Name) VALUES (@Name);";

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Name", "world_tile_grass");

                cmd.ExecuteNonQuery();
            }

            connection.Close();
        }


        //Player, camera, touch functions
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
            }






            /*
            var db = new SQLiteConnection("mybpt.db");
            TableQuery<textures> table = db.Table<textures>();
            foreach (var s in table)
            {
                texturecollection.AddTexture(s.name, Content.Load<Texture2D>(s.name));
            }

            */

            /*
            texturecollection.AddTexture("world_tile_water", Content.Load<Texture2D>("world_tile_water"));
            texturecollection.AddTexture("world_tile_sand", Content.Load<Texture2D>("world_tile_sand"));
            texturecollection.AddTexture("world_tile_grass", Content.Load<Texture2D>("world_tile_grass"));
            texturecollection.AddTexture("world_tile_stone", Content.Load<Texture2D>("world_tile_stone"));
            texturecollection.AddTexture("world_tile_snow", Content.Load<Texture2D>("world_tile_snow"));

            texturecollection.AddTexture("world_obstacle_hill", Content.Load<Texture2D>("world_obstacle_hill"));

            texturecollection.AddTexture("world_highlight_white", Content.Load<Texture2D>("world_highlight_white"));

            texturecollection.AddTexture("world_highlight_lightblue", Content.Load<Texture2D>("world_highlight_lightblue"));
            texturecollection.AddTexture("world_building_residential_1", Content.Load<Texture2D>("world_building_residential_1"));
            texturecollection.AddTexture("world_building_residential_2", Content.Load<Texture2D>("world_building_residential_2"));
            texturecollection.AddTexture("world_building_commercial_1", Content.Load<Texture2D>("world_building_commercial_1"));
            texturecollection.AddTexture("world_building_commercial_2", Content.Load<Texture2D>("world_building_commercial_2"));
            texturecollection.AddTexture("world_building_industrial_1", Content.Load<Texture2D>("world_building_industrial_1"));
            texturecollection.AddTexture("world_building_industrial_2", Content.Load<Texture2D>("world_building_industrial_2"));

            texturecollection.AddTexture("world_station_1", Content.Load<Texture2D>("world_station_1"));
            texturecollection.AddTexture("world_station_2", Content.Load<Texture2D>("world_station_2"));
            texturecollection.AddTexture("world_terminus_1", Content.Load<Texture2D>("world_terminus_1"));
            texturecollection.AddTexture("world_terminus_2", Content.Load<Texture2D>("world_terminus_2"));

            texturecollection.AddTexture("world_road_west_east", Content.Load<Texture2D>("world_road_west_east"));
            texturecollection.AddTexture("world_road_north_south", Content.Load<Texture2D>("world_road_north_south"));
            texturecollection.AddTexture("world_roadcross", Content.Load<Texture2D>("world_roadcross"));
            texturecollection.AddTexture("world_bridge_west_east", Content.Load<Texture2D>("world_bridge_west_east"));
            texturecollection.AddTexture("world_bridge_north_south", Content.Load<Texture2D>("world_bridge_north_south"));
            texturecollection.AddTexture("world_vehicle_bus_front", Content.Load<Texture2D>("world_vehicle_bus_back"));

            texturecollection.AddTexture("hud_zoom_in", Content.Load<Texture2D>("hud_zoom_in"));
            texturecollection.AddTexture("hud_zoom_out", Content.Load<Texture2D>("hud_zoom_out"));
            texturecollection.AddTexture("hud_dpad_east", Content.Load<Texture2D>("hud_dpad_east"));
            texturecollection.AddTexture("hud_dpad_north", Content.Load<Texture2D>("hud_dpad_north"));
            texturecollection.AddTexture("hud_dpad_south", Content.Load<Texture2D>("hud_dpad_south"));
            texturecollection.AddTexture("hud_dpad_west", Content.Load<Texture2D>("hud_dpad_west"));

            texturecollection.AddTexture("hud_button_buy_open", Content.Load<Texture2D>("hud_button_buy_open"));
            texturecollection.AddTexture("hud_button_buy_close", Content.Load<Texture2D>("hud_button_buy_close"));
            texturecollection.AddTexture("hud_button_buy_terminus", Content.Load<Texture2D>("hud_button_buy_terminus"));
            texturecollection.AddTexture("hud_button_buy_station", Content.Load<Texture2D>("hud_button_buy_station"));
            texturecollection.AddTexture("hud_button_station_move", Content.Load<Texture2D>("hud_button_station_move"));
            texturecollection.AddTexture("hud_button_station_upgrade", Content.Load<Texture2D>("hud_button_station_upgrade"));
            texturecollection.AddTexture("hud_button_station_sell", Content.Load<Texture2D>("hud_button_station_sell"));
            texturecollection.AddTexture("hud_button_apply", Content.Load<Texture2D>("hud_button_apply"));
            texturecollection.AddTexture("hud_button_cancel", Content.Load<Texture2D>("hud_button_cancel"));
            texturecollection.AddTexture("hud_button_demolish", Content.Load<Texture2D>("hud_button_demolish"));
            */

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
                if (idofhighlightedstation >-1)
                {
                    gameworld.Stations[idofhighlightedstation].DrawButtons(spriteBatchHud);
                    gameworld.Stations[idofhighlightedstation].DrawInfo(spriteBatchHud,font);
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
            player.AddMoney(-gameworld.Stations[highlightedstationid].UpgradeCost);
            gameworld.Stations[highlightedstationid].LevelStationUp(texturecollection, gameworld);
        }

        //Building movement
        private void MoveBuildingUp()
        {
            gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
            gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X - 1, gameworld.Stations[highlightedstationid].Coordinates.Y),gameworld);
        }
        private void MoveBuildingDown()
        {
            gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
            gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X + 1, gameworld.Stations[highlightedstationid].Coordinates.Y ), gameworld);
        }
        private void MoveBuildingLeft()
        {
            gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
            gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y - 1), gameworld);;
        }
        private void MoveBuildingRight()
        {
            gameworld.Stations[highlightedstationid].ApplyIncome(gameworld);
            gameworld.Stations[highlightedstationid].MoveStationTo(new Point(gameworld.Stations[highlightedstationid].Coordinates.X, gameworld.Stations[highlightedstationid].Coordinates.Y + 1), gameworld);
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
                        if (gameworld.IsLevelingUpAllowed())
                        {
                            CloseBuyMenu();
                            SnapCameraToSelectedTile();
                            MarkPlacementForPurchase(true);
                        }
                        else
                        {
                            //MESSAGE : NOT HIGH ENOUGH LVL
                        }

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
                            if (gameworld.Stations[i].Level!=gameworld.Stations[i].Maxlevel)
                            {
                                if (player.CanAfford(gameworld.Stations[i].UpgradeCost))
                                {
                                    gameworld.Stations[highlightedstationid].LevelStationUp(texturecollection, gameworld); 
                                }
                                else
                                {
                                    //msg: player cannot afford!
                                }
                            }
                            else
                            {
                               //msg: building already max level!
                            }
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
                            i = gameworld.Stations.Count;
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
        private void PayIncome()
        {
            if (incometimer.Timeleft < 0)
            {
                incometimer.StartTimer(incomeinterval);
                player.AddMoney(gameworld.CurrentIncome);
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
        private void UpdatePlayerLevel()
        {
            player.Level = gameworld.CountTerminiOnMap();
        }


        //Misc.
        private void ExitAppIfBackButtonIsPressed()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
        }

    }
}
