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
        Point preferredscreensize;
        Point tileSize;
        float viewdistance;

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
            stations.Add(new Station(texturecollection,gameworld,new Point(16,5),2,"Astoria",200,false,4));
            stations.Add(new Station(texturecollection, gameworld, new Point(9,5), 2, "Blaha Lujza Ter", 250, false, 3));
            stations.Add(new Station(texturecollection, gameworld, new Point(5, 5), 2, "Keleti Palyaudvar", 1000, true, 4));
        }
        protected override void UnloadContent()
        {

        }
        protected override void Update(GameTime gameTime)
        {
            UpdateTouchCollection();
            //HUD Element, Button press handlers
            CheckIfAnyStationsAreHighlighted();
            UpdateHudVisibilityBasedOnBuyMenu();
            if (tc.Count > 0)
            {
                TouchLocation currenttouchlocation = tc[0];
                HandleZoomButtonPresses(currenttouchlocation);
                HandleDpadPresses(currenttouchlocation);
                HandleBuyMenuButtonPresses(currenttouchlocation);
                HandleStationButtonPresses(currenttouchlocation);
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
            DrawStations();
            spriteBatch.End();

            spriteBatchHud.Begin(SpriteSortMode.Immediate,
                                BlendState.AlphaBlend,
                                SamplerState.PointClamp,
                                DepthStencilState.None,
                                RasterizerState.CullNone);
            DrawMainHud();
            DrawBuyMenuElements();
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
            texturecollection.AddTexture("arrow_n", Content.Load<Texture2D>("arrow_n"));
            texturecollection.AddTexture("arrow_e", Content.Load<Texture2D>("arrow_e"));
            texturecollection.AddTexture("arrow_w", Content.Load<Texture2D>("arrow_w"));
            texturecollection.AddTexture("arrow_s", Content.Load<Texture2D>("arrow_s"));
            texturecollection.AddTexture("zoomin", Content.Load<Texture2D>("zoomin"));
            texturecollection.AddTexture("zoomout", Content.Load<Texture2D>("zoomout"));
        }
        public void ReInitiateTouchCollection()
        {
            tc = new TouchCollection();
            tc = TouchPanel.GetState();
        }
        public void UpdateTouchCollection()
        {
            tc = TouchPanel.GetState();
        }
        public void SnapCameraToSelectedTile()
        {
            Tile currenttile = gameworld.MapData[gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y];
            float mapx = currenttile.Position.X-preferredscreensize.X/2+ currenttile.Texture.Width/2;
            float mapy = currenttile.Position.Y- preferredscreensize.Y / 2 + currenttile.Texture.Height / 2; ;
            camera.TargetPosition = (new Vector2(mapx, mapy));
        }
        public void UpdateIsometricCamera()
        {
            isometriccameraposition = isoCalculator.IsoTo2D(new Vector2(-camera.Position.X * 2, camera.Position.Y));
        }

        //Highlighted tile related functions
        public void UpdateHighlightedTile()
        {
            gameworld.HighlightCurrentTile(spriteBatch);
        }
        public void CheckIfAnyStationsAreHighlighted()
        {
            for (int i = 0; i < stations.Count; i++)
            {
                stations[i].CheckIfHighlighted(gameworld.CurrentTilePosition);
            }
        }
        public int IdOfHighlightedStation
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
        public void DrawMap()
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
        public void DrawStations()
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
        public void DrawZoomButtons()
        {
            zoomin.Draw(spriteBatchHud);
            zoomout.Draw(spriteBatchHud);
        }
        public void DrawDpad()
        {
            dpad_up.Draw(spriteBatchHud);
            dpad_right.Draw(spriteBatchHud);
            dpad_down.Draw(spriteBatchHud);
            dpad_left.Draw(spriteBatchHud);
        }
        public void DrawMainHud()
        {
            if (!buymenu.BuymenuIsOpen)
            {
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
        public void DrawBuyMenuElements()
        {
            if (!movingbuilding)
            {
                buymenu.Draw(spriteBatchHud);
            }
        }
        public void DrawFPS(GameTime gameTime)
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
            stations[highlightedbuildingid].Highlighted = false;
            stations[highlightedbuildingid].FinalizeMove(gameworld);
            movingbuilding = false;
            buyingbuilding = false;
        }
        private void MakeBuildingPurchase()
        {
            player.AddMoney(-stations[highlightedbuildingid].Cost);
            FinishBuildingPlacement();
            buyingbuilding = false;
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
        public void ZoomIn()
        {
            camera.Zoom = 1f;
            viewdistance = 1.1f;
        }
        public void ZoomOut()
        {
            camera.Zoom = 0.5f;
            viewdistance = 2.2f;
        }

        public void DPAD_Up()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X - 1, gameworld.CurrentTilePosition.Y));
            SnapCameraToSelectedTile();
        }
        public void DPAD_Down()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X + 1, gameworld.CurrentTilePosition.Y));
            SnapCameraToSelectedTile();
        }
        public void DPAD_Left()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y - 1));
            SnapCameraToSelectedTile();
        }
        public void DPAD_Right()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y + 1));
            SnapCameraToSelectedTile();
        }

        public void HandleZoomButtonPresses(TouchLocation currenttouchlocation)
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
        public void HandleDpadPresses(TouchLocation currenttouchlocation)
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
        public void HandleBuyMenuButtonPresses(TouchLocation currenttouchlocation)
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
                    CloseBuyMenu();
                    SnapCameraToSelectedTile();
                    MarkPlacementForPurchase(true);
                }
                if (buymenu.StationBuyButton.IsTapped(currenttouchlocation) && buymenu.StationBuyButton.Visible)
                {
                    CloseBuyMenu();
                    SnapCameraToSelectedTile();
                    MarkPlacementForPurchase(false);
                }
            }
        }
        public void HandleStationButtonPresses(TouchLocation currenttouchlocation)
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

        //Location, maths, renderdistance functions
        public int RemoveOffsetMin(int a)
        {
            if (a < 0)
            {
                return 0;
            }
            return a;
        }
        public int RemoveOffsetMax(int a, int worldsize)
        {
            if (a > worldsize)
            {
                return worldsize;
            }
            return a;
        }
        public Vector2 FindCenter(int width, int height)
        {
            return new Vector2(width / 2, height / 2);
        }

        //Buymenu logic functions
        public void OpenBuyMenu()
        {
            buymenu.OpenBuyMenu();
        }
        public void CloseBuyMenu()
        {
            buymenu.CloseBuyMenu();
        }
        public void UpdateHudVisibilityBasedOnBuyMenu()
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

        //Misc.
        public void ExitAppIfBackButtonIsPressed()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
        }
    }
}
