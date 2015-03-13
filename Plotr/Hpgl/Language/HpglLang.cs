using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Language
{
    public class HPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public HPoint()
        {

        }
        public HPoint(int x, int y)
        {
            X = x; Y = y;
        }
        public override string ToString()
        {
            return String.Format("[{0},{1}]", X, Y);
        }
        public HPoint Add(HPoint p)
        {
            return new HPoint() { X = X + p.X, Y = Y + p.Y };
        }
        public HPoint Add(int x, int y)
        {
            return new HPoint() { X = X + x, Y = Y + y };
        }
        public HPoint Sub(HPoint p)
        {
            return new HPoint() { X = X - p.X, Y = Y - p.Y };
        }
        public HPoint Mul(double scale)
        {
            return new HPoint((int)(X * scale), (int)(Y * scale));
        }

        public static double LengthAbs(HPoint p1, HPoint p2)
        {
            return Math.Abs(Length(p1, p2));
        }
        public static double Length(HPoint p1, HPoint p2)
        {
            var l2 = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            if (l2 < 0)
                return -Math.Sqrt(-l2);
            else
                return Math.Sqrt(l2);
        }

        public override bool Equals(object obj)
        {
            if (obj is HPoint)
                return Equals((HPoint)obj);
            return base.Equals(obj);
        }
        public bool Equals(HPoint p)
        {
            return X == p.X && Y == p.Y;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() % 65535; //65535 is prime number
        }
    }

    public abstract class HpglItem
    {
        public abstract string HpglStr();
    }

    public class Terminator : HpglItem
    {
        public override string HpglStr()
        {
            return ";";
        }
    }
    public class UnknownCommand : HpglItem
    {
        public string Command { get; set; }
        public override string HpglStr()
        {
            return Command;
        }
    }

    public abstract class HpglPointsCommand : HpglItem
    {
        private string name;
        public List<HPoint> Points { get; set; }
        public HpglPointsCommand(string name)
        {
            this.name = name;
            Points = new List<HPoint>();
        }
        public override string HpglStr()
        {
            return name + String.Join(",", Points.Select(p => String.Format("{0},{1}", p.X, p.Y)));
        }
    }

    public class PenUp : HpglPointsCommand
    {
        public PenUp() : base("PU") { }
    }
    public class PenDown : HpglPointsCommand
    {
        public PenDown() : base("PD") { }
    }
    public class PenAbsolute : HpglPointsCommand
    {
        public PenAbsolute() : base("PA") { }
    }
    public class PenRelative : HpglPointsCommand
    {
        public PenRelative() : base("PR") { }
    }

    public class Initialization : HpglItem
    {
        public override string HpglStr()
        {
            return "IN";
        }
    }
    public class SelectPen : HpglItem
    {
        public int Pen { get; set; }
        public override string HpglStr()
        {
            return String.Format("SP{0}", Pen);
        }
    }

    public class Label : HpglItem
    {
        public string Text { get; set; }
        public override string HpglStr()
        {
            return "LB" + Text + "\x0d";
        }
    }

    public class SetSpeed : HpglItem
    {
        public int SpeedMove { get; set; }
        public int DelayUp { get; set; }
        public override string HpglStr()
        {
            var s = SpeedMove.ToString();
            if (DelayUp != 0)
                s += "," + DelayUp;
            return "VS" + s;
        }
    }
}
