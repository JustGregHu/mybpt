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
        Random rnd = new Random();
        Tile[,] mapdata;
        Dictionary<int, Texture2D> texturecollection;

        //GAME WORLD

        public GameWorld(Dictionary<int, Texture2D> texturecollection)
        {
            perlin = new Perlin();
            worldsize = 64;
            this.texturecollection = texturecollection;
            mapdata = new Tile[worldsize,worldsize];
        }

        public void GenerateRandomWorld()
        {
            double[] dnoisemap = new double[worldsize * worldsize]; 
            for (int i = 0; i < worldsize*worldsize; i++)
            {
                perlin = new Perlin();
                dnoisemap[i] = OctavePerlin(worldsize, worldsize, 1, 16, 1);
            }
            double[,] noisemap = new double[worldsize, worldsize];
            for (int i = 0; i < worldsize; i++)
            {
                for (int p = 0; p < worldsize; p++)
                {
                    noisemap[i, p] = dnoisemap[(i + 1) * p];

                }
            }
            

            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    int rndtile = (int)Math.Round(noisemap[i,u] * (255));
                    Texture2D rndtexture;
                    if (rndtile > 150)
                    {
                        rndtexture=texturecollection[2];
                    }
                    else if (rndtile < 100)
                    {
                        rndtexture = texturecollection[0];
                    }
                    else
                    {
                        rndtexture = texturecollection[1];
                    }
                    
                    mapdata[i, u] = new Tile(rndtile,rndtexture,new Vector2((i)*100,(u)*100),new Rectangle(new Point((i+1)*100,(u+1)*100),new Point(100,100)));
                }
            }
            




        }

        public double OctavePerlin(double x, double y, double z, int octaves, double persistence)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++)
            {
                total += perlin.perlin(x * frequency, y * frequency, z * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        /*
         * 
         * 
         * 
         *         public void GenerateRandomWorld()
        {
            float[,] noisemap = GenerateNoiseMap(worldsize, worldsize, 16);
            for (int i = 0; i < worldsize; i++)
            {
                for (int u = 0; u < worldsize; u++)
                {
                    int rndtile = (int)Math.Round(noisemap[i,u] * (255));
                    Texture2D rndtexture;
                    if (rndtile > 210)
                    {
                        rndtexture=texturecollection[2];
                    }
                    else if (rndtile < 50)
                    {
                        rndtexture = texturecollection[0];
                    }
                    else
                    {
                        rndtexture = texturecollection[1];
                    }
                    mapdata[i, u] = new Tile(rndtile,rndtexture,new Vector2((i)*100,(u)*100),new Rectangle(new Point((i+1)*100,(u+1)*100),new Point(100,100)));
                }
            }
            



        }



        public float[,] GenerateNoiseMap(int x, int y, int octaves)
        {
            var data = new float[x * y];
            var min = float.MaxValue;
            var max = float.MinValue;
            Noise2d.Reseed();

            var frequency = 0.5f;
            var amplitude = 0.5f;
            var persistence = 0.25f;

            for (var octave = 0; octave < octaves; octave++)
            {
                Parallel.For(0, x * y, (offset) => {
                    int i = offset % x;
                    int j = offset / x;
                    var noise = Noise2d.Noise(i * frequency * 1f/ x, j * frequency * 1f / y);
                    noise = data[j * x + i] += noise * amplitude;
                    min = Math.Min(min, noise);
                    max = Math.Max(max, noise);

                });

                frequency *= 2;
                amplitude /= 2;
            }

            var noisemap = data.Select((f) => { var norm = (f - min) / (max - min); return norm; }).ToArray();
            float[,] fullnoisemap = new float[worldsize,worldsize];
            for (int i = 0; i < worldsize; i++)
            {
                for (int p = 0; p < worldsize; p++)
                {
                        fullnoisemap[i, p] = noisemap[(i+1) * p];
                    
                }
            }
            //float[] colors = data.Select((f) => { float norm = (f - min) / (max - min); return norm; }).ToArray();
            return fullnoisemap;
        }
        */


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

        public Dictionary<int, Texture2D> TextureCollection { get => texturecollection; set => texturecollection = value; }
    }
}