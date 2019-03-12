using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MyBPT.Classes;
using System;
using System.Collections.Generic;

namespace MyBPT
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Perlin perlin = new Perlin();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        TouchCollection tc;
        GameTextures texturecollection;
        GameWorld gameworld;
        SpriteFont font;
        Camera camera;

        public Game1()
        {
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
        protected override void Initialize()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DragComplete;
            camera = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            texturecollection = new GameTextures();
            texturecollection.AddTexture("water", Content.Load<Texture2D>("water"));
            texturecollection.AddTexture("sand", Content.Load<Texture2D>("sand"));
            texturecollection.AddTexture("grass", Content.Load<Texture2D>("grass"));
            texturecollection.AddTexture("grasstree", Content.Load<Texture2D>("grasstree"));
            texturecollection.AddTexture("stone", Content.Load<Texture2D>("stone"));
            texturecollection.AddTexture("snow", Content.Load<Texture2D>("snow"));
            gameworld = new GameWorld(texturecollection.GetTextures());
            gameworld.GenerateMap(spriteBatch, GraphicsDevice);
            font = Content.Load<SpriteFont>("regulartext");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
           
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            tc = TouchPanel.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            camera.Update(gameTime,tc);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend,null,null,null,null,camera.transform);

            
            foreach (var tile in gameworld.MapData)
            {
                tile.Highlighted = false;
            }
            
            if (tc.Count > 0)
            {
               
                if (gameworld.GetTileAtTouchPosition(tc[0])!=null)
                {
                    TouchLocation tlcameraadjusted = new TouchLocation(tc[0].Id, tc[0].State, new Vector2(tc[0].Position.X + camera.Position.X, tc[0].Position.Y + camera.Position.Y));
                    try
                    {
                        gameworld.GetTileAtTouchPosition(tlcameraadjusted).Highlighted = true; 
                    }
                    catch (System.Exception)
                    {

                    }
                }
            }


            try {
                for (int i = (int)((camera.Position.X / 100)); i < (int)((camera.Position.X + 1100)/100); i++) {
                    for (int p = (int)((camera.Position.Y / 100)); p < (int)((camera.Position.Y + 1500) / 100); p++) {
                            gameworld.MapData[i, p].Draw(spriteBatch);
                    }
                }
            } catch (Exception) {

                
            }

            spriteBatch.DrawString(font, camera.Position.X+", "+camera.Position.Y, new Vector2(50 + camera.Position.X, 50 + camera.Position.Y), Color.White);

            //DEBUG
            try {
                if (tc.Count > 0)
                {
                    if (gameworld.GetTileAtTouchPosition(tc[0]) != null)
                    {
                        TouchLocation tlcameraadjusted = new TouchLocation(tc[0].Id, tc[0].State, new Vector2(tc[0].Position.X + camera.Position.X, tc[0].Position.Y + camera.Position.Y));
                        spriteBatch.DrawString(font, gameworld.GetTileAtTouchPosition(tlcameraadjusted).Position.ToString(), new Vector2(50 + camera.Position.X, 100 + camera.Position.Y), Color.White);
                        spriteBatch.DrawString(font, gameworld.GetGridPositionAtTouchPosition(tlcameraadjusted).X + " " + gameworld.GetGridPositionAtTouchPosition(tlcameraadjusted).Y, new Vector2(50 + camera.Position.X, 150 + camera.Position.Y), Color.White);
                        spriteBatch.DrawString(font, gameworld.IsGridAvailableAt(tlcameraadjusted.Position.ToPoint()).ToString(), new Vector2(50 + camera.Position.X, 200 + camera.Position.Y), Color.White);

                    }

                }
            }
            catch (System.Exception)
            {

            }

           



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




            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
