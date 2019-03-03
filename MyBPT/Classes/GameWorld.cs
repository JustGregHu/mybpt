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

namespace MyBPT.Classes {
    class GameWorld {
        Random rnd = new Random();
        Tile[,] mapdata;
        Dictionary<int, Texture2D> texturecollection;

        //GAME WORLD

        public GameWorld(Dictionary<int, Texture2D> texturecollection)
        {
            this.texturecollection = texturecollection;
            mapdata = new Tile[32,32];
        }

        public void GenerateRandomWorld()
        {
            int rndmaxvalue = texturecollection.Count();
            for (int i = 0; i < 32; i++)
            {
                for (int u = 0; u < 32; u++)
                {
                    int rndtile = rnd.Next(0, rndmaxvalue);
                    mapdata[i, u] = new Tile(rndtile,texturecollection[rndtile],new Vector2((i)*100,(u)*100),new Rectangle(new Point((i+1)*100,(u+1)*100),new Point(100,100)));
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
    }
}