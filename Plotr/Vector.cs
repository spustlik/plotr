using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graphics2D
{
    public struct Point2D
    {
        public double X;
        public double Y;
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        public Point2D Add(Point2D p2)
        {
            return new Point2D(X + p2.X, Y + p2.Y);
        }
        public Size2D Sub(Point2D p2)
        {
            return new Size2D(X-p2.X, Y-p2.Y);
        }

        public Point2D Rotate(double angleRad)
        {
            var v = new Vector2D(new Size2D(X, Y));
            v.Angle += angleRad;
            var size = v.Size;
            return new Point2D(size.Width, size.Height);
        }
    }
    public struct Size2D
    {
        public double Width;
        public double Height;
        public Size2D(double w, double h)
        {
            Width = w;
            Height = h;
        }
    }
    public struct Vector2D
    {
        public double Length;
        public double Angle;
        public Size2D Size
        {
            get { return GetSize(Length, Angle); }
        }

        public static Size2D GetSize(double length, double angle)
        {
            return new Size2D((Math.Cos(angle) * length), (Math.Sin(angle) * length));
        }

        public Vector2D(Size2D size)
        {
            Angle = (double)Math.Atan2(size.Height, size.Width);
            Length = (double)Math.Sqrt(size.Height * size.Height + size.Width * size.Width);
        }

        public double AngleDegrees
        {
            get { return (double)(180 * Angle / Math.PI); }
            set { Angle = (double)(Math.PI * value / 180); }
        }

        public static Vector2D FromPoints(Point2D p1, Point2D p2)
        {
            return new Vector2D(new Size2D(p1.X - p2.X, p1.Y - p2.Y));
        }

    }
}
