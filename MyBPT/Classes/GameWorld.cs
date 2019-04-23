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
        Random rnd = new Random();
        Point tileSize = new Point(200, 100);
        Perlin perlin;
        int worldsize;
        double noisescale;
        Tile[,] mapdata;
        Dictionary<string, Texture2D> texturecollection;
        Texture2D texture;
        Color[] colourMap;
        double[,] noiseMap;
        Point currentTilePosition;
        Tile highlightTile;
        int waterheight = 70;
        int hillheight = 180;
        List<Road> roads = new List<Road>();
        List<Obstacle> obstacles = new List<Obstacle>();
        List<Building> buildings = new List<Building>();
        List<Station> stations = new List<Station>();
        int largeworldsize = 50;
        int smallworldsize = 28;

        public bool IsThereABuildingAt(Point coordinates)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].Coordinates == coordinates)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsThereAStationAt(Point coordinates)
        {
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i].Coordinates == coordinates)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsLevelingUpAllowed()
        {
            if (CountTerminiOnMap()==0 || CountStationsOnMap() >= CountTerminiOnMap()*5)
            {
                return true;
            }
            return false;
        }

        public int CountTerminiOnMap()
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

        public int CountStationsOnMap()
        {
            int count = 0;
            for (int i = 0; i < stations.Count; i++)
            {
                if (!stations[i].Isterminus)
                {
                    count++;
                }
            }
            return count;
        }

        public int CurrentIncome
        {
            get
            {
                int income = 0;
                for (int i = 0; i < stations.Count; i++)
                {
                    income += stations[i].Income;
                }
                return income;
            }
        }
        
        public bool IsThereanObstacleAt(Point coordinates)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i].Coordinates==coordinates)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsThereWaterAt(Point coordinates)
        {
            if (mapdata[coordinates.X, coordinates.Y].Height<waterheight)
            {
                return true;
            }
            return false;
        }

        public bool IsThereARoadNextTo(Point coordinates,int distance)
        {
            for (int i = 0; i < roads.Count; i++)
            {
                for (int x = -distance; x < distance+1; x++)
                {
                    for (int y = -distance; y < distance+1; y++)
                    {
                        Point currentposition = new Point(coordinates.X + x, coordinates.Y + y);
                        if (roads[i].Coordinates == currentposition && MapData[currentposition.X, currentposition.Y].Height>waterheight)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsThereARoadAt(Point coordinates)

        {
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i].Coordinates == coordinates)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AreThereHillsInX(int x)
        {
            for (int i = 0; i < worldsize; i++)
            {
                if (mapdata[x,i].Height>=hillheight)
                {
                    return true;
                }
            }
            return false;
        }
        public bool AreThereHillsInY(int y)
        {
            for (int i = 0; i < worldsize; i++)
            {
                if (mapdata[i,y ].Height >= hillheight)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateCurrentTile(Point newposition)
        {
            // if worldsize smaller bigger 0, etc. ganme needs a jump to highlight button!
            try
            {
                highlightTile.Position = MapData[newposition.X, newposition.Y].Position;
                highlightTile.Area = MapData[newposition.X, newposition.Y].Area;
                currentTilePosition = newposition;
            }
            catch(Exception){}

        }
        public void HighlightCurrentTile(SpriteBatch spriteBatch)
        {
            highlightTile.Draw(spriteBatch);      
        }
        public void InitiateHighlightTile()
        {
            highlightTile = new Tile(0, texturecollection["world_highlight_white"], mapdata[currentTilePosition.X, currentTilePosition.Y].Position, mapdata[currentTilePosition.X, currentTilePosition.Y].Area);
        }

        public Point CurrentTilePosition{get{return currentTilePosition;}set{currentTilePosition = value;}}

        //GAME WORLD


        public GameWorld(Dictionary<string, Texture2D> texturecollection,bool size)
        {
            perlin = new Perlin();
            if (size)
            {
                worldsize = smallworldsize;
            }
            else
            {
                worldsize = largeworldsize;
            }
            currentTilePosition = new Point(worldsize / 2, worldsize / 2);
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
                    int height = (int)Math.Round(noiseMap[i, u] * (255));
                    Texture2D rndtexture;
                    if (height < waterheight)
                    {
                        rndtexture = texturecollection["world_tile_water"];
                    }
                    else if (height < 75)
                    {
                      rndtexture = texturecollection["world_tile_sand"];
                        
                    }
                    else if (height < 170)
                    {
                        rndtexture = texturecollection["world_tile_grass"];
                    }
                    else if (height < 190)
                    {
                        rndtexture = texturecollection["world_tile_stone"];
                    }
                    else if (height < 255)
                    {
                        rndtexture = texturecollection["world_tile_snow"];
                    }
                    else
                    {
                        rndtexture = texturecollection["world_tile_water"];
                    }

                    int draw_x = (int)(u * 100);
                    int draw_y = (int)(i * 100);
                    Point temppoint = isoCalculator.TwoDToIso(new Point(draw_x, draw_y));
                    mapdata[i, u] = new Tile(height, rndtexture, new Vector2(temppoint.X, temppoint.Y), new Rectangle(temppoint, (new Point(100, 65))));
                }
        }

            List<int> roadpositionsX = new List<int>();
            List<int> roadpositionsY = new List<int>();
            for (int i = 0; i < rnd.Next(3,6); i++)
            {
                int newrand=(rnd.Next(1, worldsize - 1));
                bool randisnew = true;
                for (int u = 0; u < roadpositionsX.Count; u++)
                {
                    if (newrand == roadpositionsX[u])
                    {
                        randisnew = false;
                    }
                }
                if (randisnew)
                {
                    roadpositionsX.Add(newrand);
                }
               
            }
            for (int i = 0; i < rnd.Next(3,6); i++)
            {
                int newrand = (rnd.Next(1, worldsize - 1));
                bool randisnew = true;
                for (int u = 0; u < roadpositionsY.Count; u++)
                {
                    if (newrand==roadpositionsY[u])
                    {
                        randisnew = false; ;
                    }
                }
                if (randisnew)
                {
                    roadpositionsY.Add(newrand);
                }
            }

            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    if (MapData[i, u].Height > hillheight)
                        obstacles.Add(new Obstacle(texturecollection, "world_obstacle_hill", this, new Point(i, u), 500));
                    for (int x = 0; x < roadpositionsX.Count; x++)
                    {
                        if (i == roadpositionsX[x] && !AreThereHillsInX(roadpositionsX[x]))
                        {
                            if (MapData[i, u].Height < waterheight)
                            {
                                roads.Add(new Road(texturecollection, "world_bridge_west_east", this, new Point(i, u)));
                            }
                            else if (MapData[i, u].Height > hillheight)
                            {
                                //hegyre nem helyez utat
                            }
                            else
                            {
                                roads.Add(new Road(texturecollection, "world_road_west_east", this, new Point(i, u)));
                            }
                        }
                    }
                    for (int y = 0; y < roadpositionsY.Count; y++)
                    {
                        if (u == roadpositionsY[y] && !AreThereHillsInY(roadpositionsY[y]))
                        {
                            if (IsThereARoadAt(new Point(i, u)))
                            {
                                roads.Add(new Road(texturecollection, "world_roadcross", this, new Point(i, u)));
                            }
                            else
                            {
                                if (MapData[i, u].Height < waterheight)
                                {
                                    roads.Add(new Road(texturecollection, "world_bridge_north_south", this, new Point(i, u)));
                                }
                                else if (MapData[i, u].Height > 180)
                                {
                                    //hegyre nem helyez utat
                                }
                                else
                                {
                                    roads.Add(new Road(texturecollection, "world_road_north_south", this, new Point(i, u)));
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    if (!IsThereARoadAt(new Point(i, u)) && !IsThereanObstacleAt(new Point(i, u)) && !IsThereWaterAt(new Point(i, u))&&IsThereARoadNextTo(new Point(i,u),6))
                    {
                        int type = 1;
                        int level = 1;
                        int random1 = rnd.Next(0, 101); 
                        int random2 = rnd.Next(0, 101);
                        if (random1 < 65)
                        {
                            type = 0;
                        }
                        else if (random1 < 90)
                        {
                            type = 1;
                        }
                        else
                        {
                            type = 2;
                        }
                        if (random2 > 80)
                        {
                            level = 2;
                        }
                        if (MapData[i, u].Height < 160 && MapData[i, u].Height > 90)
                        {
                            if (rnd.Next(1, 100) > 85)
                            {
                                buildings.Add(new Building(texturecollection, this, new Point(i, u),type,level));

                            }
                        }
                        else if (MapData[i, u].Height > 160)
                        {
                            if (rnd.Next(1, 100) > 95)
                            {
                                buildings.Add(new Building(texturecollection, this, new Point(i, u), type, level));

                            }
                        }
                        else if (MapData[i, u].Height < 90)
                        {
                            if (rnd.Next(1, 100) > 90)
                            {
                                buildings.Add(new Building(texturecollection, this, new Point(i, u), type, level));

                            }


                        }
                    }
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
        internal List<Obstacle> Obstacles { get => obstacles; set => obstacles = value; }
        internal List<Road> Roads { get => roads; set => roads = value; }
        internal List<Building> Buildings { get => buildings; set => buildings = value; }
        internal List<Station> Stations { get => stations; set => stations = value; }
    }
}