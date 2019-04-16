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
using Microsoft.Xna.Framework;

namespace MyBPT.Classes
{
    class IsoCalculator
    {
        public Point IsoTo2D(Point pointtoconvert)
        {
            Point temppoint = new Point(0, 0);
            temppoint.X = (2 * pointtoconvert.Y + pointtoconvert.X / 2);
            temppoint.Y = (2 * pointtoconvert.Y - pointtoconvert.X / 2);
            return temppoint;
        }

        public Point TwoDToIso(Point pointtoconvert)
        {
            Point temppoint = new Point(0, 0);
            temppoint.X = pointtoconvert.X - pointtoconvert.Y;
            temppoint.Y = (pointtoconvert.X + pointtoconvert.Y) / 2;
            return temppoint;
        }

        public Vector2 IsoTo2D(Vector2 vectortoconvert)
        {
            Vector2 temppoint = new Vector2(0, 0);
            temppoint.X = (2 * vectortoconvert.Y + vectortoconvert.X / 2);
            temppoint.Y = (2 * vectortoconvert.Y - vectortoconvert.X / 2);
            return temppoint;
        }

        public Vector2 TwoDToIso(Vector2 vectortoconvert)
        {
            Vector2 temppoint = new Vector2(0, 0);
            temppoint.X = vectortoconvert.X - vectortoconvert.Y;
            temppoint.Y = (vectortoconvert.X + vectortoconvert.Y) / 2;
            return temppoint;
        }
    }
}