using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Transformations
{
    /// <summary>
    /// transforms PU,PD,PA points by transformation matrix
    /// PR transformation is done without translation, but it can be strange - due to rotation around some absolute point
    /// it is better, if <see cref="Absolutizer"/> is used before this
    /// </summary>
    public class Transformer : HpglVisitor
    {        
        private List<HpglItem> result;
        private HMatrix transformMatrix;

        public Transformer()
        {
            transformMatrix = new HMatrix(HMatrix.Identity);
        }

        public Transformer(string transformation) : this()
        {
            var parts = transformation.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var par = part.Split(new string[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);
                par[0] = par[0].ToLowerInvariant();
                if (par.Length!=2)
                    throw new ArgumentException(par[0]);
                if (par[0]=="movex" || par[0] == "mx")
                {
                    Move(Int32.Parse(par[1]), 0);
                }
                else if (par[0]=="movey" || par[0] == "my")
                {
                    Move(0, Int32.Parse(par[1]));
                }
                else if (par[0].StartsWith("rot"))
                {
                    Rotate(Int32.Parse(par[1]));
                }
                else if (par[0] == "scalex" || par[0] == "zoomx")
                {
                    var z = double.Parse(par[1]);
                    Scale(z, 1);
                }
                else if (par[0] == "scaley" || par[0] == "zoomy")
                {
                    var z = double.Parse(par[1]);
                    Scale(1, z);
                }
                else if (par[0] == "scale" || par[0] == "zoom")
                {
                    var z = double.Parse(par[1]);
                    Scale(z, z);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(par[0]);
                }
            }

        }

        public void Scale(double z1, double z2)
        {
            transformMatrix = transformMatrix.Mul(
                new HMatrix(
                    z1, 0, 0,
                    0, z2, 0,
                    0, 0, 1));
        }
        internal void Scale(double p)
        {
            Scale(p, p);
        }

        public void Rotate(int rotate)
        {
            var a = -rotate / 180.0d * Math.PI;
            transformMatrix = transformMatrix.Mul(
                new HMatrix(
                    Math.Cos(a), -Math.Sin(a), 0,
                    Math.Sin(a), Math.Cos(a), 0,
                    0, 0, 1));
        }

        public void Move(int x, int y)
        {
            transformMatrix = transformMatrix.Mul(
                new HMatrix(
                    1, 0, 0, 
                    0, 1, 0, 
                    x, y, 1));
        }

        public List<HpglItem> Transform(List<HpglItem> items)
        {
            if (transformMatrix.Equals(HMatrix.Identity))
                return items;
            Console.WriteLine("Transforming...");
            result = new List<HpglItem>();
            Visit(items);
            return result;
        }

        private HPoint TransformPoint(HPoint p)
        {
            var r = transformMatrix.Mul(p);
            return r;
        }

        private HPoint TransformPointRelative(HPoint p)
        {
            var t = new HMatrix(transformMatrix);
            t[2, 0] = 0;
            t[2, 1] = 0;
            return t.Mul(p);
        }
        
        protected override void Unprocessed(HpglItem item)
        {
            result.Add(item);
        }

        protected override void VisitPenUp(PenUp item)
        {
            TrasformPoints(item);
        }

        private void TrasformPoints(HpglPointsCommand item)
        {
            for (int i = 0; i < item.Points.Count; i++)
            {
                item.Points[i] = TransformPoint(item.Points[i]);
            }
            result.Add(item);
        }

        protected override void VisitPenDown(PenDown item)
        {
            TrasformPoints(item);
        }

        protected override void VisitPenAbsolute(PenAbsolute item)
        {
            TrasformPoints(item);
        }

        protected override void VisitPenRelative(PenRelative item)
        {
            Console.WriteLine("Warning:PR transformation not supported, results may be ugly");
            for (int i = 0; i < item.Points.Count; i++)
            {
                item.Points[i] = TransformPointRelative(item.Points[i]);
            }
            result.Add(item);
        }

    }
}
