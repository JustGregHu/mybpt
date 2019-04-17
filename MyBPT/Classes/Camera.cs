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
using com.bitbull.meat;
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
        Vector2 targetposition;
        float zoomamount;
        Lerper lerper = new Lerper();

        public Camera(Viewport newView)
        {
            targetposition = new Vector2(0, 0);
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
                        targetposition = new Vector2(targetposition.X - gs.Delta.X, targetposition.Y - gs.Delta.Y);
                    }
                    else if (GestureType.Pinch == gs.GestureType)
                    {
                        //ZOOM UNIMPLEMENTED
                        this.Zoom += gs.Delta.X/1000;
                        this.Zoom += gs.Delta.Y/ 1000;
                    }
                }
            }
            position.X= lerper.Lerp(position.X, targetposition.X);
            position.Y = lerper.Lerp(position.Y, targetposition.Y);
            transform = Matrix.CreateTranslation(new Vector3(-position.X-view.Width / 2, -position.Y-view.Height / 2, 0))*Matrix.CreateScale(new Vector3(Zoom,Zoom,0))*Matrix.CreateTranslation(new Vector3(view.Width / 2, view.Height / 2, 0));

        }

        public Vector2 TargetPosition { get => targetposition; set => targetposition = value; }

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
                /*if (zoomamount < 0.1f) zoomamount = 0.1f;
                if (zoomamount > 2f) zoomamount = 2f;*/
            }
        }
    }
}