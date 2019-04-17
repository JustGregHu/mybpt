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
        IsoCalculator isoCalculator = new IsoCalculator();
        Point tileSize = new Point(200, 100);
        Perlin perlin;
        int worldsize;
        double noisescale;
        Random rnd = new Random();
        Tile[,] mapdata;
        Dictionary<string, Texture2D> texturecollection;
        Texture2D texture;
        Color[] colourMap;
        double[,] noiseMap;
        Point currentTilePosition = new Point(1,1);
        Tile highlightTile;

        public void UpdateCurrentTile(Point newposition)
        {
            // if worldsize smaller bigger 0, etc. ganme needs a jump to highlight button!
            try
            {
                highlightTile.Position = MapData[newposition.X, newposition.Y].Position;
                highlightTile.Area = MapData[newposition.X, newposition.Y].Area;
                currentTilePosition = newposition;
            }
            catch(Exception e){}

        }
        public void HighlightCurrentTile(SpriteBatch spriteBatch)
        {
            highlightTile.Draw(spriteBatch);      
        }
        public void InitiateHighlightTile()
        {
            highlightTile = new Tile(0, texturecollection["highlight"], mapdata[currentTilePosition.X, currentTilePosition.Y].Position, mapdata[currentTilePosition.X, currentTilePosition.Y].Area);
        }

        public Point CurrentTilePosition{get{return currentTilePosition;}set{currentTilePosition = value;}}

        //GAME WORLD


        public GameWorld(Dictionary<string, Texture2D> texturecollection)
        {
            perlin = new Perlin();
            worldsize = 128;
            noisescale = 12f;
            this.texturecollection = texturecollection;
            mapdata = new Tile[worldsize, worldsize];        }

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

        public void DrawNoiseMap(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(0, 0), Color.White);
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
                    int height = (int)Math.Round(noiseMap[i, u] * (255));
                    Texture2D rndtexture;
                    if (height < 90)
                    {
                        rndtexture = texturecollection["stone"];
                    }
                    else if (height < 190)
                    {
                      rndtexture = texturecollection["grass"];
                        
                    }
                    else if (height > 190)
                    {
                        rndtexture = texturecollection["snow"];
                    }
                    else
                    {
                        rndtexture = texturecollection["grass"];
                    }

                    int draw_x = (int)(u * 100);
                    int draw_y = (int)(i * 100);
                    Point temppoint = isoCalculator.TwoDToIso(new Point(draw_x, draw_y));
                    mapdata[i, u] = new Tile(height, rndtexture, new Vector2(temppoint.X, temppoint.Y), new Rectangle(temppoint, (new Point(100, 65)))); 
            }
        }
    } 

        //GRID AVAILABILITY, SNAPPING
        public bool IsGridAvailableAt(Point gridlocation)
        {
            if (mapdata[(int)(gridlocation.X / tileSize.X), (int)(gridlocation.Y / tileSize.Y)].Height==0)
            {
                return true;
            }
            return false;
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
        public int Worldsize { get => worldsize; set => worldsize = value; }
    }
}