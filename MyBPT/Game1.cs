using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MyBPT.Classes;
using System.Collections.Generic;

namespace MyBPT
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        TouchCollection tc;

        Tile selectedtile=null;

        List<Tile> gameworld;

        SpriteFont font;
        int debugscore;

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
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            gameworld = new List<Tile>();
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Vector2 dirtfloortemppos=new Vector2(200,200);
            gameworld.Add(new Tile(Content.Load<Texture2D>("grass"), dirtfloortemppos, new Rectangle(dirtfloortemppos.ToPoint(), new Point(100, 100))));
            dirtfloortemppos = new Vector2(200, 300);
            gameworld.Add(new Tile(Content.Load<Texture2D>("grass"), dirtfloortemppos, new Rectangle(dirtfloortemppos.ToPoint(), new Point(100, 100))));
            dirtfloortemppos = new Vector2(400, 300);
            gameworld.Add(new Tile(Content.Load<Texture2D>("pearl"), dirtfloortemppos, new Rectangle(dirtfloortemppos.ToPoint(), new Point(100, 100))));
            font = Content.Load<SpriteFont>("regulartext");
            debugscore = 0;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {


            // TODO: Add your drawing code here

            


            tc = TouchPanel.GetState();

            spriteBatch.Begin();
            foreach (var tile in gameworld) {
                tile.Draw(spriteBatch);
                if (tc.Count>0) {
                    
                    //movingtile
                    //tile.MoveIfTouchIsHeld(spriteBatch, tc[0]);

                }
            }

            if (TouchPanel.GetCapabilities().IsConnected)
            {
                spriteBatch.DrawString(font, "touchscreen detected", new Vector2(50, 50), Color.White);
                spriteBatch.DrawString(font, "distance moved: "+debugscore.ToString(), new Vector2(50, 100), Color.White);
            }
            else
            {
                spriteBatch.DrawString(font, "no touchscreen detected", new Vector2(50, 50), Color.White);
            }
            

            

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
