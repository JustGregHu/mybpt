using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MyBPT.Classes {
    /// <summary>
    /// A játékteret és az azzal kapcsolatos funkciókat megvalósító osztály. Terepet, épületeket, akadályokat, pályaelemeket generál, amellyekkel interaktálhat a játékos.
    /// </summary>
    class GameWorld {
        #region Változók, Pályaelemek, Tulajdonságok
        IsoCalculator isoCalculator = new IsoCalculator();
        Random rnd = new Random();
        Tile[,] mapdata;
        GameTextures texturecollection;
        Texture2D texture;
        Color[] colourMap;
        Point currentTilePosition;
        Tile highlightTile;
        Perlin perlin;
        double noisescale;
        int worldsize;
        int waterheight = 70;
        int hillheight = 180;
        int largeworldsize = 50;
        int smallworldsize = 28;
        Point tileSize = new Point(200, 100);

        //Pályaelemek
        double[,] noiseMap;
        List<Road> roads = new List<Road>();
        List<Obstacle> obstacles = new List<Obstacle>();
        List<Building> buildings = new List<Building>();
        List<Station> stations = new List<Station>();

        //Tulajdonságok
        public Tile[,] MapData
        {
            get
            {
                return mapdata;
            }
        }
        public GameTextures GameTextures { get => texturecollection; set => texturecollection = value; }
        public int Worldsize { get => worldsize; set => worldsize = value; }
        internal List<Obstacle> Obstacles { get => obstacles; set => obstacles = value; }
        internal List<Road> Roads { get => roads; set => roads = value; }
        internal List<Building> Buildings { get => buildings; set => buildings = value; }
        internal List<Station> Stations { get => stations; set => stations = value; }
        public Point CurrentTilePosition { get => currentTilePosition; set => currentTilePosition = value; }

        #endregion
        #region Játékvilág létrehozása

        /// <summary>
        /// Létrehoz egy játékvilág objektumot. Inicializál egy üres világot.
        /// </summary>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="size">Ha igaz, kisebb méretű világ generálódik. Normál méretű, ha hamis</param>
        public GameWorld(GameTextures texturecollection, bool size)
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

        /// <summary>
        /// Új térkép létrehozása, világgeneráláshoz szükséges funkció
        /// </summary>
        /// <param name="graphicsdevice">MonoGame-hez tartozó grafikai készlet</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="graphicsdevice">MonoGame-hez tartozó grafikai készlet</param>
        public void GenerateMap(SpriteBatch spriteBatch, Point preferredscreensize, GraphicsDevice graphicsDevice)
        {
            double[,] noiseMap = GenerateNoiseMap(worldsize, worldsize, noisescale);
            GenerateRandomWorld(preferredscreensize,texturecollection);
            CreateNoiseMap(spriteBatch, graphicsDevice, noiseMap);
        }

        /// <summary>
        /// Visszatér egy perlin zajtérkép koordinátáival
        /// </summary>
        /// <param name="mapHeight">A térkép magassága</param>
        /// <param name="mapWidth">A térkép szélessége</param>
        /// <param name="scale">Zajerősség mértéke</param>
        public double[,] GenerateNoiseMap(int mapWidth, int mapHeight, double scale)
        {
            this.noiseMap = new double[mapWidth, mapHeight];

            if (scale <= 0)
            {
                scale = 0.001f;
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    double sampleX = x / scale;
                    double sampleY = y / scale;
                    double perlinValue = perlin.perlin(sampleX, sampleY, 1);
                    noiseMap[x, y] = perlinValue;
                }
            }
            return noiseMap;
        }

        /// <summary>
        /// Létrehozza és eltárolja a zajtérképet, színerősséggel kifejezve
        /// </summary>
        /// <param name="graphicsdevice">MonoGame-hez tartozó grafikai készlet</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="noiseMap">GenerateNoiseMap-ból készített zaj koordináta térkép</param>
        public void CreateNoiseMap(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, double[,] noiseMap)
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

        /// <summary>
        /// Legenerál és elment egy új világot. Létrehozza a csempetérképet, majd az utakat, akadályokat és az épületeket.
        /// </summary>
        /// /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        public void GenerateRandomWorld(Point preferredscreensize,GameTextures gameTextures)
        {
            //csempék
            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    int height = (int)Math.Round(noiseMap[i, u] * (255));
                    Texture2D rndtexture;
                    if (height < waterheight)
                    {
                        rndtexture = texturecollection.GetTextures()["world_tile_water"];
                    }
                    else if (height < 80)
                    {
                        rndtexture = texturecollection.GetTextures()["world_tile_sand"];

                    }
                    else if (height < 170)
                    {
                        rndtexture = texturecollection.GetTextures()["world_tile_grass"];
                    }
                    else if (height < 190)
                    {
                        rndtexture = texturecollection.GetTextures()["world_tile_stone"];
                    }
                    else if (height < 255)
                    {
                        rndtexture = texturecollection.GetTextures()["world_tile_snow"];
                    }
                    else
                    {
                        rndtexture = texturecollection.GetTextures()["world_tile_water"];
                    }

                    int draw_x = (int)(u * 100);
                    int draw_y = (int)(i * 100);
                    Point temppoint = isoCalculator.TwoDToIso(new Point(draw_x, draw_y));
                    mapdata[i, u] = new Tile(height, rndtexture, new Vector2(temppoint.X, temppoint.Y), new Rectangle(temppoint, (new Point(100, 65))));
                }
            }
            
            //utak x, majd utak y

            List<int> roadpositionsX = new List<int>();
            List<int> roadpositionsY = new List<int>();
            for (int i = 0; i < rnd.Next(3, 6); i++)
            {
                int newrand = (rnd.Next(1, worldsize - 1));
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
            for (int i = 0; i < rnd.Next(3, 6); i++)
            {
                int newrand = (rnd.Next(1, worldsize - 1));
                bool randisnew = true;
                for (int u = 0; u < roadpositionsY.Count; u++)
                {
                    if (newrand == roadpositionsY[u])
                    {
                        randisnew = false; ;
                    }
                }
                if (randisnew)
                {
                    roadpositionsY.Add(newrand);
                }
            }

            //blokádok, hidak és kereszteződések

            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    if (MapData[i, u].Height > hillheight)
                        obstacles.Add(new Obstacle(texturecollection.GetTextures(), "world_obstacle_hill", preferredscreensize, this, new Point(i, u), 500));
                    for (int x = 0; x < roadpositionsX.Count; x++)
                    {
                        if (i == roadpositionsX[x] && !AreThereHillsInX(roadpositionsX[x]))
                        {
                            if (MapData[i, u].Height < waterheight)
                            {
                                roads.Add(new Road(texturecollection.GetTextures(), "world_bridge_west_east", this, new Point(i, u)));
                            }
                            else if (MapData[i, u].Height > hillheight)
                            {
                                //hegyre nem helyez utat
                            }
                            else
                            {
                                roads.Add(new Road(texturecollection.GetTextures(), "world_road_west_east", this, new Point(i, u)));
                            }
                        }
                    }
                    for (int y = 0; y < roadpositionsY.Count; y++)
                    {
                        if (u == roadpositionsY[y] && !AreThereHillsInY(roadpositionsY[y]))
                        {
                            if (IsThereARoadAt(new Point(i, u)))
                            {
                                roads.Add(new Road(texturecollection.GetTextures(), "world_roadcross", this, new Point(i, u)));
                            }
                            else
                            {
                                if (MapData[i, u].Height < waterheight)
                                {
                                    roads.Add(new Road(texturecollection.GetTextures(), "world_bridge_north_south", this, new Point(i, u)));
                                }
                                else if (MapData[i, u].Height > 180)
                                {
                                    //hegyre nem helyez utat
                                }
                                else
                                {
                                    roads.Add(new Road(texturecollection.GetTextures(), "world_road_north_south", this, new Point(i, u)));
                                }
                            }
                        }
                    }
                }
            }

            //épületek

            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    if (!IsThereARoadAt(new Point(i, u)) && !IsThereanObstacleAt(new Point(i, u)) && !IsThereWaterAt(new Point(i, u)) && IsThereARoadNextTo(new Point(i, u), 6))
                    {
                        int type = 1;
                        int level = 1;
                        int random1 = rnd.Next(0, 101);
                        int random2 = rnd.Next(0, 101);
                        if (random1 < 65)
                        {
                            type = 1;
                        }
                        else if (random1 < 90)
                        {
                            type = 2;
                        }
                        else
                        {
                            type = 3;
                        }
                        if (random2 > 80)
                        {
                            level = 2;
                        }
                        if (MapData[i, u].Height < 160 && MapData[i, u].Height > 90)
                        {
                            if (rnd.Next(1, 100) > 85)
                            {
                                buildings.Add(new Building(texturecollection, this, preferredscreensize, new Point(i, u), type, level));

                            }
                        }
                        else if (MapData[i, u].Height > 160)
                        {
                            if (rnd.Next(1, 100) > 95)
                            {
                                buildings.Add(new Building(texturecollection, this, preferredscreensize, new Point(i, u), type, level));

                            }
                        }
                        else if (MapData[i, u].Height < 90)
                        {
                            if (rnd.Next(1, 100) > 90)
                            {
                                buildings.Add(new Building(texturecollection, this, preferredscreensize, new Point(i, u), type, level));

                            }


                        }
                    }
                }
            }
        }

        #endregion
        #region Keresés, Megszámlálás, Információ

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordinátát még nem foglalta el csempe
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
        public bool IsGridEmptyAt(Point coordinates)
        {
            if (mapdata[(int)(coordinates.X / tileSize.X), (int)(coordinates.Y / tileSize.Y)].Height == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordinátát egy víz textúrát tartalmaző csempe foglalja el
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
        public bool IsThereWaterAt(Point coordinates)
        {
            if (mapdata[coordinates.X, coordinates.Y].Height < waterheight)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordinátát vízszintesen és függőlegesen körülvevő koordináták közül legalább az egyik tartalmaz útobjektumot 
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
        /// <param name="distance">A megadott koordinátától való kiinduló távolság. 1 = 1 csempe távolsága </param>
        public bool IsThereARoadNextTo(Point coordinates, int distance)
        {
            for (int i = 0; i < roads.Count; i++)
            {
                for (int x = -distance; x < distance + 1; x++)
                {
                    for (int y = -distance; y < distance + 1; y++)
                    {
                        Point currentposition = new Point(coordinates.X + x, coordinates.Y + y);
                        if (roads[i].Coordinates == currentposition && MapData[currentposition.X, currentposition.Y].Height > waterheight)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordináta alatt található útobjektum
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
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

        /// <summary>
        /// Igazzal tér vissza, ha a megadott vízszintes irányú sor bármely csemépje tartalmaz blokádot
        /// </summary>
        /// <param name="x">Az ellenőrizendő sor</param>
        public bool AreThereHillsInX(int x)
        {
            for (int i = 0; i < worldsize; i++)
            {
                if (mapdata[x, i].Height >= hillheight)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott függőleges irányú sor bármely csemépje tartalmaz blokádot
        /// </summary>
        /// <param name="y">Az ellenőrizendő sor</param>
        public bool AreThereHillsInY(int y)
        {
            for (int i = 0; i < worldsize; i++)
            {
                if (mapdata[i, y].Height >= hillheight)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordináta alatt blokádobjektum található 
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
        public bool IsThereanObstacleAt(Point coordinates)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i].Coordinates == coordinates)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordináta alatt épület található
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
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

        /// <summary>
        /// Igazzal tér vissza, ha a megadott koordináta alatt állomás található
        /// </summary>
        /// <param name="coordinates">Az ellenőrizendő koordináta</param>
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

        /// <summary>
        /// Igazzal tér vissza, ha engedélyezett az újabb végállomások megvásárlása. Az állomások és a végállomások aránya legyen legalább 5 állomás : 1 végállomás
        /// </summary>
        public bool IsLevelingUpAllowed()
        {
            if (CountTerminiOnMap()==0 || CountStationsOnMap() >= CountTerminiOnMap()*5)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Visszatér a pályán található végállomások számával
        /// </summary>
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

        /// <summary>
        /// Visszatér a pályán található állomások számával
        /// </summary>
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

        /// <summary>
        /// Visszatér az állomások által elért bevételek összevégel
        /// </summary>
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

        #endregion
        #region Kurzor, Jelenlegi csempe műveletek

        /// <summary>
        /// Létrehozza, és alapbeállításba helyezi a kurzort
        /// </summary>
        public void InitiateHighlightTile()
        {
            highlightTile = new Tile(0, texturecollection.GetTextures()["world_highlight_white"], mapdata[currentTilePosition.X, currentTilePosition.Y].Position, mapdata[currentTilePosition.X, currentTilePosition.Y].Area);
        }

        /// <summary>
        /// Frissíti a kurzor / jelenleg kiválasztott csempe pozícióját
        /// </summary>
        /// <param name="newposition"></param>
        public void UpdateCurrentTile(Point newposition)
        {
            try
            {
                highlightTile.Position = MapData[newposition.X, newposition.Y].Position;
                highlightTile.Area = MapData[newposition.X, newposition.Y].Area;
                currentTilePosition = newposition;
            }
            catch(Exception){}

        }

        /// <summary>
        /// Megjeleníti a kurzort a jelenleg kiválasztott csempe körül
        /// </summary>
        /// <param name = "spriteBatch" > MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void HighlightCurrentTile(SpriteBatch spriteBatch)
        {
            highlightTile.Draw(spriteBatch);      
        }

        #endregion
        #region Műveletek
        /// <summary>
        /// double-t alakít float-tá
        /// </summary>
        /// <param name="value">Konvtertálandó érték</param>
        public static float ToSingle(double value)
        {
            return (float)value;
        }
        #endregion
    }
}