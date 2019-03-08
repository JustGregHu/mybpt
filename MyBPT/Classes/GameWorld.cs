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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MyBPT.Classes;
using System.Threading.Tasks;

namespace MyBPT.Classes {
    class GameWorld {
        
        Perlin perlin;
        int worldsize;
        double noisescale;
        Random rnd = new Random();
        Tile[,] mapdata;
        Dictionary<string, Texture2D> texturecollection;
        Texture2D texture;
        Color[] colourMap;
        double[,] noiseMap;


        //GAME WORLD

        public GameWorld(Dictionary<string, Texture2D> texturecollection)
        {
            perlin = new Perlin();
            worldsize = 64;
            noisescale = 12f;
            this.texturecollection = texturecollection;
            mapdata = new Tile[worldsize, worldsize];
        }

        public double[,] GenerateNoiseMap(int mapWidth, int mapHeight, double scale)
        {
            this.noiseMap = new double[mapWidth,mapHeight];

            if (scale<=0)
            {
                scale = 0.001f;
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    double sampleX = x/scale;
                    double sampleY = y/scale;
                    double perlinValue = perlin.perlin(sampleX, sampleY, 1);
                    noiseMap[x, y] = perlinValue;
                }
            }
            return noiseMap;
        }

        public void GenerateMap(SpriteBatch spriteBatch,GraphicsDevice graphicsDevice)
        {
            double[,] noiseMap = GenerateNoiseMap(worldsize, worldsize, noisescale);
            GenerateRandomWorld();
            CreateNoiseMap(spriteBatch,graphicsDevice,noiseMap);

        }

        public void CreateNoiseMap(SpriteBatch spriteBatch,GraphicsDevice graphicsDevice, double[,] noiseMap)
        {
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);
           
            this.texture = new Texture2D(graphicsDevice, width, height);
            this.colourMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colourMap[y * width + x] = Color.Lerp(Color.Black, Color.White, ToSingle(noiseMap[x, y]));
                }
            }
            this.texture.SetData(colourMap);
        }

        public void DrawNoiseMap(Camera camera, SpriteFont font, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(0, 0), Color.White);
            for (int i = 0; i < 5; i++)
            {
                spriteBatch.DrawString(font, noiseMap[i,1].ToString(), new Vector2(50 + camera.Position.X, 150 + camera.Position.Y+i*30), Color.White);
            }
        }
        public static float ToSingle(double value)
        {
            return (float)value;
        }

        
    public void GenerateRandomWorld()
    {
        for (int i = 0; i < worldsize; i++)
        {
            for (int u = 0; u < worldsize; u++)
            {
                    int newrnd = rnd.Next(0, 6);
                    int rndtile = (int)Math.Round(noiseMap[i, u] * (255));
                    Texture2D rndtexture;
                    if (rndtile < 80)
                    {
                        rndtexture = texturecollection["water"];
                    }
                    else if (rndtile <100)
                    {
                        rndtexture = texturecollection["sand"];
                    }
                    else if (rndtile < 160)
                    {
                        if (newrnd==5)
                        {
                            rndtexture = texturecollection["grasstree"];
                        }
                        else
                        {
                            rndtexture = texturecollection["grass"];
                        }
                        
                    }
                    else if (rndtile < 190)
                    {
                        rndtexture = texturecollection["stone"];
                    }
                    else if (rndtile < 256)
                    {
                        rndtexture = texturecollection["snow"];
                    }
                    else
                    {
                        rndtexture = texturecollection["water"];
                    }

                    mapdata[i, u] = new Tile(rndtile, rndtexture, new Vector2((i) * 100, (u) * 100), new Rectangle(new Point((i + 1) * 100, (u + 1) * 100), new Point(100, 100)));
                mapdata[i, u] = new Tile(rndtile,rndtexture,new Vector2((i)*100,(u)*100),new Rectangle(new Point((i+1)*100,(u+1)*100),new Point(100,100)));
            }
        }

    } 

        //TILE-GRID POSITIONS

        public Vector2 GetTilePositionAtGridLocation(Point gridlocation)
        {
            return new Vector2(gridlocation.X*100,gridlocation.Y*100);
        }


        public Tile GetTileAtTouchPosition(TouchLocation tl)
        {
            try
            {
                return mapdata[(int)(tl.Position.X / 100), (int)(tl.Position.Y / 100)];
            }
            catch (Exception)
            {
                return null;
                throw;
            }

        }


        public Point GetGridPositionAtTouchPosition(TouchLocation tl)
        {
            return new Point((int)(tl.Position.X) / 100, (int)(tl.Position.Y) / 100);
        }


        //GRID AVAILABILITY, SNAPPING


        public bool IsGridAvailableAt(Point gridlocation)
        {
            if (mapdata[(int)(gridlocation.X / 100), (int)(gridlocation.Y / 100)].TextureID==0)
            {
                return true;
            }
            return false;
        }

        public void SnapToGridIfAvailable()
        {
            //NOT IMPLEMENTED YET
        }


        //ACCESSORS

        public Tile[,] MapData
        {
            get
            {
                return mapdata;
            }
        }

        public Dictionary<string, Texture2D> TextureCollection { get => texturecollection; set => texturecollection = value; }
    }
}