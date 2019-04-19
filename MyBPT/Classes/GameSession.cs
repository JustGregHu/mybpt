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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MyBPT.Classes;
using Android.Content.PM;

namespace MyBPT.Classes
{


    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameSession : Game
    {
        IsoCalculator isoCalculator = new IsoCalculator();
        Point tileSize = new Point(200, 100);
        FrameCounter frameCounter = new FrameCounter();
        float viewdistance;
        Perlin perlin = new Perlin();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatchHud;
        TouchCollection tc;
        GameTextures texturecollection;
        GameWorld gameworld;
        SpriteFont font;
        Camera camera;
        Camera hud;
        Button zoomin;
        Button zoomout;
        Button dpad_up;
        Button dpad_down;
        Button dpad_left;
        Button dpad_right;
        BuyMenu buymenu;
        Point preferredscreensize = new Point(1280, 720);
        List<Station> stations = new List<Station>();
        List<Station> termini = new List<Station>();
        Player player = new Player("testperson",1000,1);
        bool movingbuilding = false;
        bool buyingbuilding = false;

        Station testbuilding;

#if DEBUG

#endif

        public GameSession()
        {
            graphics = new GraphicsDeviceManager(this);
            try { Content.RootDirectory = "Content"; }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = preferredscreensize.X;
            graphics.PreferredBackBufferHeight = preferredscreensize.Y;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            viewdistance = 1.1f;
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            camera = new Camera(GraphicsDevice.Viewport);
            hud = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHud = new SpriteBatch(GraphicsDevice);
            texturecollection = new GameTextures();
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
            gameworld = new GameWorld(texturecollection.GetTextures());
            gameworld.GenerateMap(spriteBatch, GraphicsDevice);
            gameworld.InitiateHighlightTile();
            SnapCameraToSelectedTile();
            font = Content.Load<SpriteFont>("regulartext");

            buymenu = new BuyMenu(GraphicsDevice, preferredscreensize, texturecollection);

            int zoomwidth = texturecollection.GetTextures()["zoomout"].Width;
            int hudmargin = 50;
            zoomin = new Button(new Vector2(preferredscreensize.X - (zoomwidth + hudmargin), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["zoomin"]);
            zoomout = new Button(new Vector2(preferredscreensize.X-((zoomwidth + hudmargin) * 2), preferredscreensize.Y - zoomwidth - hudmargin), texturecollection.GetTextures()["zoomout"]);

            dpad_left = new Button(new Vector2(hudmargin-20, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["arrow_w"]);
            dpad_right = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 70 - hudmargin), texturecollection.GetTextures()["arrow_e"]);
            dpad_up = new Button(new Vector2( 120 + hudmargin, preferredscreensize.Y - 220 - hudmargin), texturecollection.GetTextures()["arrow_n"]);
            dpad_down = new Button(new Vector2(hudmargin-20, preferredscreensize.Y -70 -hudmargin), texturecollection.GetTextures()["arrow_s"]);

            stations.Add(new Station(texturecollection,gameworld,new Point(16,5),2,"Astoria",100,false,4));
            stations.Add(new Station(texturecollection, gameworld, new Point(9,5), 2, "Blaha Lujza Ter", 100, false, 3));
            stations.Add(new Station(texturecollection, gameworld, new Point(5, 5), 2, "Keleti Palyaudvar", 100, true, 4));
        }

        public void SnapCameraToSelectedTile()
        {
            Tile currenttile = gameworld.MapData[gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y];
            float mapx = currenttile.Position.X-preferredscreensize.X/2+ currenttile.Texture.Width/2;
            float mapy = currenttile.Position.Y- preferredscreensize.Y / 2 + currenttile.Texture.Height / 2; ;
            camera.TargetPosition = (new Vector2(mapx, mapy));
        }




        public void BeginBuildingPlacement()
        {
            movingbuilding = true;
        }

        public void FinishBuildingPlacement()
        {
            movingbuilding = false;
            buyingbuilding = false;
        }

        public void MarkPlacementForPurchase()
        {
            buyingbuilding = true;
        }

        public void MakeBuildingPurchase()
        {
            buyingbuilding = false;
        }

        

        public void OpenBuyMenu()
        {
            buymenu.OpenBuyMenu();
        }
        public void CloseBuyMenu()
        {
            buymenu.CloseBuyMenu();
        }
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

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            for (int i = 0; i < stations.Count; i++)
            {
                stations[i].CheckIfHighlighted(gameworld.CurrentTilePosition);

            }


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
            

            tc = TouchPanel.GetState();
            if (tc.Count > 0)
            {
                if (zoomin.IsTapped(tc[0]) && zoomin.Visible)
                {
                    ZoomIn();
                }
                if (zoomout.IsTapped(tc[0]) && zoomout.Visible)
                {
                    ZoomOut();
                }
                if (dpad_up.IsTapped(tc[0]) && dpad_up.Visible)
                {
                    DPAD_Up();
                }
                if (dpad_down.IsTapped(tc[0]) && dpad_down.Visible)
                {
                    DPAD_Down();
                }
                if (dpad_left.IsTapped(tc[0]) && dpad_left.Visible)
                {
                    DPAD_Left();
                }
                if (dpad_right.IsTapped(tc[0]) && dpad_right.Visible)
                {
                    DPAD_Right();
                }
                if (buymenu.OpenButton.IsTapped(tc[0]) && buymenu.OpenButton.Visible)
                {
                    OpenBuyMenu();
                }
                if (buymenu.CloseButton.IsTapped(tc[0]) && buymenu.CloseButton.Visible)
                {
                    CloseBuyMenu();
                    SnapCameraToSelectedTile();
                }
            }

           // player.UpdatePlayerLevel(terminus count);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            camera.Update(gameTime, tc);
            base.Update(gameTime);
        }

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

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform);

            Vector2 isocamera = isoCalculator.IsoTo2D(new Vector2(-camera.Position.X * 2, camera.Position.Y));

            try
            {
                for (int i = RemoveOffsetMin((int)(((isocamera.X - (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X))); i < RemoveOffsetMax((int)((((isocamera.X + graphics.PreferredBackBufferWidth + tileSize.X) + (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X)), gameworld.Worldsize); i++)
                {

                    for (int p = RemoveOffsetMin((int)(((isocamera.Y - (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X))); p < RemoveOffsetMax((int)((((isocamera.Y + graphics.PreferredBackBufferWidth + tileSize.X) + (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X)), gameworld.Worldsize); p++)
                    {
                        gameworld.MapData[i, p].Draw(spriteBatch);
                    }
                }
            }
            catch (Exception) { }
            gameworld.HighlightCurrentTile(spriteBatch);

            for (int i = 0; i < stations.Count; i++)
            {
                stations[i].Draw(spriteBatch);
            }


            spriteBatch.End();

            spriteBatchHud.Begin(SpriteSortMode.Immediate,
                                BlendState.AlphaBlend,
                                SamplerState.PointClamp,
                                DepthStencilState.None,
                                RasterizerState.CullNone);
            if (!buymenu.BuymenuIsOpen)
            {
                zoomin.Draw(spriteBatchHud);
                zoomout.Draw(spriteBatchHud);
                dpad_up.Draw(spriteBatchHud);
                dpad_right.Draw(spriteBatchHud);
                dpad_down.Draw(spriteBatchHud);
                dpad_left.Draw(spriteBatchHud);

                bool stationhighlighted = false;
                for (int i = 0; i < stations.Count; i++)
                {
                    if (stations[i].Highlighted)
                    {
                        stationhighlighted = true;
                        if (stations[i].Isterminus)
                        {
                            spriteBatchHud.DrawString(font, "Terminus", new Vector2(50, 50), Color.White);
                        }
                        else
                        {
                            spriteBatchHud.DrawString(font, "Station", new Vector2(50, 50), Color.White);
                        }
                        spriteBatchHud.DrawString(font, stations[i].GetStationType(), new Vector2(50, 100), Color.White);
                        spriteBatchHud.DrawString(font, "Influence : " + stations[i].Effectradius.ToString() + " blocks", new Vector2(50, 150), Color.White);
                        spriteBatchHud.DrawString(font, stations[i].Description, new Vector2(50, 200), Color.White);
                        stations[i].MoveButton.Draw(spriteBatchHud);
                        stations[i].SellButton.Draw(spriteBatchHud);
                        stations[i].UpgradeButton.Draw(spriteBatchHud);
                    }
                }
                if(!stationhighlighted){
                    spriteBatchHud.DrawString(font, player.Name + "   Lvl " + player.Level, new Vector2(50, 50), Color.White);
                    spriteBatchHud.DrawString(font, "Cash: " + player.Money, new Vector2(50, 100), Color.White);
                }


            }
            buymenu.Draw(spriteBatchHud);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);
            /* debuginfo..
            spriteBatchHud.DrawString(font, fps, new Vector2(50, 200), Color.White);
            spriteBatchHud.DrawString(font, "Width: " + graphics.PreferredBackBufferWidth.ToString(), new Vector2(50, 250), Color.White);
            spriteBatchHud.DrawString(font, "Height: " + graphics.PreferredBackBufferHeight.ToString(), new Vector2(50, 300), Color.White);
            spriteBatchHud.DrawString(font, camera.Position.X + ", " + camera.Position.Y, new Vector2(50, 50), Color.White);
            */


            spriteBatchHud.End();
            base.Draw(gameTime);
        }
    }
}
