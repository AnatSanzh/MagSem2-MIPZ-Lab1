using System;

namespace MagSem2_MIPZ_Lab1
{
    struct Rect
    {
        public int MinX;
        public int MinY;

        public int MaxX;
        public int MaxY;

        public float CenterX => (MinX + MaxX) * 0.5f;
        public float CenterY => (MinY + MaxY) * 0.5f;
        public int Width => MaxX - MinX;
        public int Height => MaxY - MinY;


        public static Rect GetBoundingRect(Rect rect1, Rect rect2)
        {
            return new Rect()
            {
                MinX = Math.Min(rect1.MinX, rect2.MinX),
                MinY = Math.Min(rect1.MinY, rect2.MinY),

                MaxX = Math.Max(rect1.MaxX, rect2.MaxX),
                MaxY = Math.Max(rect1.MaxY, rect2.MaxY)
            };
        }

        // manh(x,y) = |x| + |y|
        // eucl(x,y) = (x^2 + y^2)^0.5
        public static float GetManhattanDistanceBetween(Rect rect1, Rect rect2)
        {
            return Math.Max(Math.Abs(rect1.CenterX - rect2.CenterX) - (rect1.Width + rect2.Width) * 0.5f, 0) +
                Math.Max(Math.Abs(rect1.CenterY - rect2.CenterY) - (rect1.Height + rect2.Height) * 0.5f, 0);
        }

        public static bool IsTouching(Rect rect1, Rect rect2)
        {
            return GetManhattanDistanceBetween(rect1, rect2) == 1;
        }
    }
}
