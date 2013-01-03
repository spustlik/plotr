using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Transformations
{
    /// <summary>
    /// optimizes pen UP moves
    /// simple alghoritm:
    /// distance=0
    /// - while there is some point < distance, connect lines
    /// distance*=2 (or +=maxstep)
    /// </summary>
    public class SegmentationOptimizer : Optimizer
    {
        public int MaxStep = 64;
        class Segment
        {
            private List<Segment> segments = new List<Segment>();
            public Segment()
            {
            }
            public Segment(Line line)
            {
                segments.Add(new Segment() { A = line.P1, B = line.P2, Line = line });
                //Line = line; 
                A = line.P1; B = line.P2;
            }
            public HPoint A { get; set; }
            public HPoint B { get; set; }
            public Line Line { get; private set; }
            public IEnumerable<Segment> Segments { get { return segments; } }
            public override string ToString()
            {
                return String.Format("Segment {0}..{1}, ({2}) items", A, B, segments.Count);
            }
            public Segment Reversed()
            {
                var s = new Segment();
                s.A = B;
                s.B = A;
                foreach (var item in segments)
                {
                    s.segments.Add(item.Reversed());
                }
                s.segments.Reverse();
                if (Line != null)
                {
                    s.Line = new Line(Line.P2, Line.P1, Line.Attribs);
                }
                return s;
            }

            private bool IsNear(HPoint a, HPoint b, double maxDestination)
            {
                if (maxDestination < 1)
                {
                    return a.X == b.X && a.Y == b.Y;
                }
                else
                {
                    //optimalizace
                    //if (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) > maxDestination)
                    //    return false;
                    return HPoint.LengthAbs(a, b) < maxDestination;
                }
            }
            public bool AddIfNear(Segment segment, double maxDestination)
            {
                if (IsNear(A, segment.A, maxDestination))
                {
                    segments.Insert(0, segment);
                    A = segment.A;
                    return true;
                }
                if (IsNear(B, segment.A, maxDestination))
                {
                    segments.Add(segment);
                    B = segment.B;
                    return true;
                }
                if (IsNear(A, segment.B, maxDestination))
                {
                    var rev = segment.Reversed();
                    segments.Insert(0, rev);
                    A = rev.A;
                    return true;
                }
                if (IsNear(B, segment.B, maxDestination))
                {
                    var rev = segment.Reversed();
                    segments.Add(rev);
                    B = rev.B;
                    return true;
                }
                return false;
            }
            public void ExtractLines(List<Line> result)
            {
                if (Line != null)
                {
                    result.Add(Line);
                }
                foreach (var item in segments)
                {
                    item.ExtractLines(result);
                }
            }
        }

        protected override void Optimize()
        {
            //pokud existuje bod ve vzdalenosti 1, spoj a priste z toho bude novy 
            //zvetsi vzdalenost
            double max = 0.5;
            var newSegments = new List<Segment>();
            foreach (var line in segments)
            {
                var s = new Segment(line);
                newSegments.Add(s);
            }

            while (newSegments.Count > 1)
            {
                int success = 0;
                for (int i = 0; i < newSegments.Count; i++)
                {
                    int j = i + 1;
                    while (j < newSegments.Count)
                    {
                        if (newSegments[i].AddIfNear(newSegments[j], max))
                        {
                            newSegments.RemoveAt(j);
                            success++;
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
                if (success > 0)
                {
                    Console.WriteLine("Optimized {0} segments for length {1}", success, max);
                }
                if (max < MaxStep)
                {
                    max = max * 2;
                }
                else
                {
                    max += MaxStep;
                }
            }
            segments.Clear();
            foreach (var s in newSegments)
            {
                s.ExtractLines(segments);
            }
        }
    }
}
