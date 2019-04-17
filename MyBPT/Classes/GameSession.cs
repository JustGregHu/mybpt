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

namespace MyBPT.Classes {

    
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameSession : Game {
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

        #if DEBUG

        #endif

        public GameSession() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            viewdistance = 1f;
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            camera = new Camera(GraphicsDevice.Viewport);
            //camera.TargetPosition = new Vector2(-500,3000); //SORT OF CENTER CAMERA WITH THIS
            hud = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHud = new SpriteBatch(GraphicsDevice);
            texturecollection = new GameTextures();
            texturecollection.AddTexture("stone", Content.Load<Texture2D>("stone"));
            texturecollection.AddTexture("snow", Content.Load<Texture2D>("snow"));
            texturecollection.AddTexture("grass", Content.Load<Texture2D>("grass"));
            texturecollection.AddTexture("highlight", Content.Load<Texture2D>("highlight"));
            texturecollection.AddTexture("arrow_n", Content.Load<Texture2D>("arrow_n"));
            texturecollection.AddTexture("arrow_e", Content.Load<Texture2D>("arrow_e"));
            texturecollection.AddTexture("arrow_w", Content.Load<Texture2D>("arrow_w"));
            texturecollection.AddTexture("arrow_s", Content.Load<Texture2D>("arrow_s"));
            texturecollection.AddTexture("zoomin", Content.Load<Texture2D>("zoomin"));
            texturecollection.AddTexture("zoomout", Content.Load<Texture2D>("zoomout"));
            gameworld = new GameWorld(texturecollection.GetTextures());
            gameworld.GenerateMap(spriteBatch, GraphicsDevice);
            gameworld.InitiateHighlightTile();
            font = Content.Load<SpriteFont>("regulartext");

            int zoomwidth = texturecollection.GetTextures()["zoomout"].Width;
            int hudmargin = 50;
            zoomin = new Button(new Vector2(zoomwidth+hudmargin, graphics.PreferredBackBufferHeight - zoomwidth - hudmargin), texturecollection.GetTextures()["zoomin"]);
            zoomout = new Button(new Vector2((zoomwidth + hudmargin)*2, graphics.PreferredBackBufferHeight - zoomwidth-hudmargin), texturecollection.GetTextures()["zoomout"]);


            dpad_left = new Button(new Vector2(graphics.PreferredBackBufferWidth - 300- hudmargin, graphics.PreferredBackBufferHeight -300- hudmargin), texturecollection.GetTextures()["arrow_w"]);
            dpad_right = new Button(new Vector2(graphics.PreferredBackBufferWidth - 150- hudmargin, graphics.PreferredBackBufferHeight - 150- hudmargin), texturecollection.GetTextures()["arrow_e"]);
            dpad_up= new Button(new Vector2(graphics.PreferredBackBufferWidth - 150- hudmargin, graphics.PreferredBackBufferHeight -300- hudmargin), texturecollection.GetTextures()["arrow_n"]);
            dpad_down = new Button(new Vector2(graphics.PreferredBackBufferWidth - 300- hudmargin, graphics.PreferredBackBufferHeight -150- hudmargin), texturecollection.GetTextures()["arrow_s"]);

        }


        public void ZoomIn() {
            camera.Zoom = 1f;
            viewdistance = 1f;
        }
        public void ZoomOut() {
            camera.Zoom = 0.5f;
            viewdistance = 2f;
        }
        public void DPAD_Up()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X-1, gameworld.CurrentTilePosition.Y));
        }
        public void DPAD_Down()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X+1, gameworld.CurrentTilePosition.Y));
        }
        public void DPAD_Left()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y-1));
        }
        public void DPAD_Right()
        {
            gameworld.UpdateCurrentTile(new Point(gameworld.CurrentTilePosition.X, gameworld.CurrentTilePosition.Y+1));
        }

        protected override void UnloadContent() {

        }

        protected override void Update(GameTime gameTime) {
            tc = TouchPanel.GetState();
            if (tc.Count > 0) {
                if (zoomin.IsTapped(tc[0])) {
                    ZoomIn();
                }
                if (zoomout.IsTapped(tc[0])) {
                    ZoomOut();
                }
                if (dpad_up.IsTapped(tc[0]))
                {
                    DPAD_Up();
                }
                if (dpad_down.IsTapped(tc[0]))
                {
                    DPAD_Down();
                }
                if (dpad_left.IsTapped(tc[0]))
                {
                    DPAD_Left();
                }
                if (dpad_right.IsTapped(tc[0]))
                {
                    DPAD_Right();
                }
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            camera.Update(gameTime, tc);
            base.Update(gameTime);
        }

        public int RemoveOffsetMin(int a) {
            if (a<0) {
                return 0;
            }
            return a;
        }

        public int RemoveOffsetMax(int a,int worldsize) {
            if (a > worldsize) {
                return worldsize;
            }
            return a;
        }

        public Vector2 FindCenter(int width,int height) {
            return new Vector2(width/2,height/2);
        }

        protected override void Draw(GameTime gameTime) {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform);

            Vector2 isocamera = isoCalculator.IsoTo2D(new Vector2(-camera.Position.X*2, camera.Position.Y));

            try {
                for (int i = RemoveOffsetMin((int)(((isocamera.X - (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X))); i < RemoveOffsetMax((int)((((isocamera.X + graphics.PreferredBackBufferWidth + tileSize.X) + (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X) ),gameworld.Worldsize) ; i++) {

                    for (int p = RemoveOffsetMin((int)(((isocamera.Y - (graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X))); p < RemoveOffsetMax((int)((((isocamera.Y + graphics.PreferredBackBufferWidth + tileSize.X) +(graphics.PreferredBackBufferWidth * viewdistance)) / tileSize.X)), gameworld.Worldsize); p++) {
                        gameworld.MapData[i, p].Draw(spriteBatch);
                    }
                }
            } catch (Exception) {}
            gameworld.HighlightCurrentTile(spriteBatch);
            spriteBatch.End();

            spriteBatchHud.Begin(SpriteSortMode.Immediate,
                                BlendState.AlphaBlend,
                                SamplerState.PointClamp,
                                DepthStencilState.None,
                                RasterizerState.CullNone);
            zoomin.Draw(spriteBatchHud);
            zoomout.Draw(spriteBatchHud);
            dpad_up.Draw(spriteBatchHud);
            dpad_right.Draw(spriteBatchHud);
            dpad_down.Draw(spriteBatchHud);
            dpad_left.Draw(spriteBatchHud);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);
            spriteBatchHud.DrawString(font, fps, new Vector2(50,200), Color.White);
            spriteBatchHud.DrawString(font, "Width: " + graphics.PreferredBackBufferWidth.ToString(), new Vector2(50, 250), Color.White);
            spriteBatchHud.DrawString(font, "Height: " + graphics.PreferredBackBufferHeight.ToString(), new Vector2(50, 300), Color.White);
            spriteBatchHud.DrawString(font, camera.Position.X + ", " + camera.Position.Y, new Vector2(50, 50), Color.White);
            spriteBatchHud.End();
            base.Draw(gameTime);
        }
    }
}
