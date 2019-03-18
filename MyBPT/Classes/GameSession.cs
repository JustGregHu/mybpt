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
using System;
using System.Collections.Generic;

namespace MyBPT.Classes {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameSession : Game {
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

        public GameSession() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            viewdistance = 0f;
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            camera = new Camera(GraphicsDevice.Viewport);
            hud = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHud = new SpriteBatch(GraphicsDevice);
            texturecollection = new GameTextures();
            texturecollection.AddTexture("water", Content.Load<Texture2D>("water"));
            texturecollection.AddTexture("sand", Content.Load<Texture2D>("sand"));
            texturecollection.AddTexture("grass", Content.Load<Texture2D>("grass"));
            texturecollection.AddTexture("grasstree", Content.Load<Texture2D>("grasstree"));
            texturecollection.AddTexture("stone", Content.Load<Texture2D>("stone"));
            texturecollection.AddTexture("snow", Content.Load<Texture2D>("snow"));
            texturecollection.AddTexture("zoomin", Content.Load<Texture2D>("zoomin"));
            texturecollection.AddTexture("zoomout", Content.Load<Texture2D>("zoomout"));
            gameworld = new GameWorld(texturecollection.GetTextures());
            gameworld.GenerateMap(spriteBatch, GraphicsDevice);
            font = Content.Load<SpriteFont>("regulartext");
            zoomin = new Button(new Vector2(50, 900), texturecollection.GetTextures()["zoomin"]);
            zoomout = new Button(new Vector2(120, 900), texturecollection.GetTextures()["zoomout"]);
        }


        public void ZoomIn() {
            camera.Zoom = 1f;
            viewdistance = 0f;
        }
        public void ZoomOut() {
            camera.Zoom = 0.5f;
            viewdistance = 2f;
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            tc = TouchPanel.GetState();
            if (tc.Count > 0) {
                if (zoomin.IsTapped(tc[0])) {
                    ZoomIn();
                }
                if (zoomout.IsTapped(tc[0])) {
                    ZoomOut();
                }
            }


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            camera.Update(gameTime, tc);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform);









            foreach (var tile in gameworld.MapData) {
                tile.Highlighted = false;
            }
            if (tc.Count > 0) {

                if (gameworld.GetTileAtTouchPosition(tc[0]) != null) {
                    TouchLocation tlcameraadjusted = new TouchLocation(tc[0].Id, tc[0].State, new Vector2(tc[0].Position.X + camera.Position.X, tc[0].Position.Y + camera.Position.Y));
                    try {
                        gameworld.GetTileAtTouchPosition(tlcameraadjusted).Highlighted = true;
                    } catch (System.Exception) {

                    }
                }
            }


            //REWRITE THIS PART WITH RELATIVE VIEWPORT SIZES!!!!!!!!!! viewdistance figure out
            try {
                for (int i = (int)((camera.Position.X / 100)); i < (int)((((camera.Position.X + 1100)+(1100*viewdistance)) / 100) ) ; i++) {
                    for (int p = (int)((camera.Position.Y / 100)); p < (int)((((camera.Position.Y + 1500)+(1500*viewdistance)) / 100)); p++) {
                        gameworld.MapData[i, p].Draw(spriteBatch);
                    }
                }
            } catch (Exception) {


            }


            //DEBUG





            {
                // MOVING TILES

                /*

                List<Tile> movingtiles = new List<Tile>();
                ;
                foreach (var tile in gameworld.MapData) {
                    tile.Draw(spriteBatch);
                    if (tc.Count>0) {
                        spriteBatch.DrawString(font, tc[0].Position.ToString(), new Vector2(50, 50), Color.White);
                        if (tile.Moving || tile.IsTileOnPosition(tc[0].Position))
                        {
                            movingtiles.Add(tile);
                        }


                    }
                }

                if (movingtiles.Count>0)
                {
                    foreach (var tile in movingtiles)
                    {
                        tile.CheckIfReleased(tc[0]);
                    }
                    movingtiles[0].MoveIfTouchIsHeld(gameworld,spriteBatch, tc[0]);
                }
                /*


                /*if (TouchPanel.GetCapabilities().IsConnected)
                {
                    spriteBatch.DrawString(font, "touchscreen detected", new Vector2(50, 50), Color.White);
                    spriteBatch.DrawString(font, "distance moved: " + debugscore.ToString(), new Vector2(50, 100), Color.White);
                }
                else
                {
                    spriteBatch.DrawString(font, "no touchscreen detected", new Vector2(50, 50), Color.White);
                }*/

            }


            spriteBatch.End();



            spriteBatchHud.Begin(SpriteSortMode.Immediate,
                                BlendState.AlphaBlend,
                                SamplerState.PointClamp,
                                DepthStencilState.None,
                                RasterizerState.CullNone);
            
            zoomin.Draw(spriteBatchHud);
            zoomout.Draw(spriteBatchHud);


            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            frameCounter.Update(deltaTime);

            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

            spriteBatchHud.DrawString(font, fps, new Vector2(50,200), Color.White);

            spriteBatchHud.DrawString(font, camera.Position.X + ", " + camera.Position.Y, new Vector2(50, 50), Color.White);


            try {
                if (tc.Count > 0) {
                    if (gameworld.GetTileAtTouchPosition(tc[0]) != null) {
                        TouchLocation tlcameraadjusted = new TouchLocation(tc[0].Id, tc[0].State, new Vector2(tc[0].Position.X, tc[0].Position.Y));
                        spriteBatchHud.DrawString(font, gameworld.GetTileAtTouchPosition(tlcameraadjusted).Position.ToString(), new Vector2(50, 100 ), Color.White);
                        spriteBatchHud.DrawString(font, gameworld.GetGridPositionAtTouchPosition(tlcameraadjusted).X + " " + gameworld.GetGridPositionAtTouchPosition(tlcameraadjusted).Y, new Vector2(50, 150 ), Color.White);
                        spriteBatchHud.DrawString(font, gameworld.IsGridAvailableAt(tlcameraadjusted.Position.ToPoint()).ToString(), new Vector2(50, 20), Color.White);

                    }

                }
            } catch (System.Exception) {

            }

            spriteBatchHud.End();

            base.Draw(gameTime);
        }
    }
}
