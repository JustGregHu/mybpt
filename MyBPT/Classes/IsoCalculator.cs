using Microsoft.Xna.Framework;

namespace MyBPT.Classes
{
    /// <summary>
    /// 2:1 arányú izometrikus és 1:1 arányú 2D koordinátákat konvertáló osztály
    /// </summary>
    class IsoCalculator
    {
        /// <summary>
        /// 2:1 arányú Izometrikus koordinátákat alakít át 1:1 arányú 2D koordinátákká
        /// </summary>
        public Point IsoTo2D(Point pointtoconvert)
        {
            Point temppoint = new Point(0, 0);
            temppoint.X = (2 * pointtoconvert.Y + pointtoconvert.X / 2);
            temppoint.Y = (2 * pointtoconvert.Y - pointtoconvert.X / 2);
            return temppoint;
        }

        /// <summary>
        /// 1:1 arányú 2D koordinátákat alakít át 2:1 arányú Izometrikus koordinátákká
        /// </summary>
        public Point TwoDToIso(Point pointtoconvert)
        {
            Point temppoint = new Point(0, 0);
            temppoint.X = pointtoconvert.X - pointtoconvert.Y;
            temppoint.Y = (pointtoconvert.X + pointtoconvert.Y) / 2;
            return temppoint;
        }

        /// <summary>
        /// 2:1 arányú Izometrikus koordinátákat alakít át 1:1 arányú 2D koordinátákká
        /// </summary>
        public Vector2 IsoTo2D(Vector2 vectortoconvert)
        {
            Vector2 temppoint = new Vector2(0, 0);
            temppoint.X = (2 * vectortoconvert.Y + vectortoconvert.X / 2);
            temppoint.Y = (2 * vectortoconvert.Y - vectortoconvert.X / 2);
            return temppoint;
        }

        /// <summary>
        /// 1:1 arányú 2D koordinátákat alakít át 2:1 arányú Izometrikus koordinátákká
        /// </summary>
        public Vector2 TwoDToIso(Vector2 vectortoconvert)
        {
            Vector2 temppoint = new Vector2(0, 0);
            temppoint.X = vectortoconvert.X - vectortoconvert.Y;
            temppoint.Y = (vectortoconvert.X + vectortoconvert.Y) / 2;
            return temppoint;
        }
    }
}