using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Transformations
{
    public class HMatrix
    {
        private static HMatrix _identity = new HMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1);
        public static HMatrix Identity { get { return _identity;} }

        private double[,] values;
        public double this[int x, int y] { get { return values[x, y]; } set { values[x, y] = value; } }

        public HMatrix()
        {
            values = new double[3, 3] { { 0d, 0d, 0d }, { 0d, 0d, 0d }, { 0d, 0d, 0d } };
        }

        public HMatrix(double p1, double p2, double p3, double p4, double p5, double p6, double p7, double p8, double p9)
            : this()
        {
            values[0, 0] = p1;
            values[0, 1] = p2;
            values[0, 2] = p3;
            values[1, 0] = p4;
            values[1, 1] = p5;
            values[1, 2] = p6;
            values[2, 0] = p7;
            values[2, 1] = p8;
            values[2, 2] = p9;
        }

        public HMatrix(HMatrix source) : this()
        {
            Array.Copy(source.values, this.values, source.values.LongLength);
        }

        public HMatrix Mul(HMatrix by)
        {
            var result = new HMatrix();
            var r = result.values;
            var a = this.values;
            var b = by.values;
            r[0, 0] = a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0];
            r[0, 1] = a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1];
            r[0, 2] = a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2];

            r[1, 0] = a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0];
            r[1, 1] = a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1];
            r[1, 2] = a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2];

            r[2, 0] = a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0];
            r[2, 1] = a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1];
            r[2, 2] = a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2];
            return result;
        }

        public HPoint Mul(HPoint p)
        {
            return new HPoint()
            {
                X = (int)(values[0, 0] * p.X + values[1, 0] * p.Y + values[2, 0] * 1),
                Y = (int)(values[0, 1] * p.X + values[1, 1] * p.Y + values[2, 1] * 1)
            };
        }

        public override int GetHashCode()
        {
            return values.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var m = obj as HMatrix;
            if (m != null)
                return Equals(m);
            return base.Equals(obj);
        }

        public bool Equals(HMatrix m)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (this[x, y] != m[x, y])
                        return false;
                }
            }
            return true;
        }

    }
}
