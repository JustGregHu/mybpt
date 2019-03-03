﻿using System;
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
using Microsoft.Xna.Framework.Input.Touch;

namespace MyBPT.Classes
{
    class Camera
    {
        public Matrix transform;
        Viewport view;
        Vector2 position;
        float zoomamount;

        public Camera(Viewport newView)
        {
            position = new Vector2(0, 0);
            view = newView;
            zoomamount = 1;
        }

        public void Update(GameTime gameTime,TouchCollection tc)
        {

            if (tc.Count>0)
            {
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (GestureType.FreeDrag == gs.GestureType)
                    {
                        position = new Vector2(position.X - gs.Delta.X, position.Y - gs.Delta.Y);
                    }
                    else if (GestureType.Pinch == gs.GestureType)
                    {
                        //ZOOM UNIMPLEMENTED
                        //zoomamount += gs.Delta.X/1000;
                    }
                }
            }
            
            transform = Matrix.CreateTranslation(new Vector3(-position.X-view.Width / 2, -position.Y-view.Height / 2, 0))*Matrix.CreateScale(new Vector3(Zoom,Zoom,0))*Matrix.CreateTranslation(new Vector3(view.Width/2,view.Height/2,0));

        }

        public Vector2 Position { get => position; set => position = value; }
        public float Zoom
        {
            get
            {
                return zoomamount;
            }
            set
            {
                zoomamount = value;
                //this does not work .. ?
                if (zoomamount < 0.5f) zoomamount = 0.5f;
                if (zoomamount > 2f) zoomamount = 2f;
            }
        }
    }
}