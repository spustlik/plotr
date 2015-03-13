using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Transformations
{

    /// <summary>
    /// converts instructions to lines
    /// </summary>
    public abstract class Optimizer : HpglAllVisitor
    {
        bool isPenDown = false;
        HPoint current = new HPoint();
        Attributes currentAttribs = new Attributes();
        protected List<Line> segments = new List<Line>();

        public List<HpglItem> Process(List<HpglItem> items)
        {
            Visit(items);
            Optimize();
            return SegmentsToHpgl();
        }

        protected abstract void Optimize();

        private List<HpglItem> SegmentsToHpgl()
        {
            var result = new List<HpglItem>();
            result.Add(new Initialization());
            var last = new HPoint();
            foreach (var line in segments)
            {
                if (last != line.P1 || result.Count == 0)
                {
                    result.Add(new PenUp() { Points = { line.P1 } });
                    result.Add(new PenDown() { Points = { line.P2 } });
                }
                else
                {
                    var lastDown = (PenDown)result.Last();
                    lastDown.Points.Add(line.P2);
                }
                last = line.P2;
            }
            result.Add(new PenUp());
            return result;
        }

        protected override void VisitTerminator(Terminator item)
        {
        }

        protected override void VisitPenUp(PenUp item)
        {
            isPenDown = false;
            if (item.Points.Count > 0)
                current = item.Points.Last();
        }

        protected override void VisitPenDown(PenDown item)
        {
            foreach (var p in item.Points)
            {
                if (isPenDown && p.Equals(current))
                {
                    Console.WriteLine("Optimized dup");
                    continue;
                }
                isPenDown = true;
                segments.Add(new Line(current, p, currentAttribs));
                current = p;
            }
        }

        protected override void VisitPenAbsolute(PenAbsolute item)
        {
            if (isPenDown)
            {
                foreach (var p in item.Points)
                {
                    segments.Add(new Line(current, p, currentAttribs));
                    current = p;
                }
            }
            else
            {
                if (item.Points.Count > 0)
                {
                    current = item.Points.Last();
                }
            }
        }

        protected override void VisitPenRelative(PenRelative item)
        {
            if (isPenDown)
            {
                foreach (var rp in item.Points)
                {
                    var p = current.Add(rp);
                    segments.Add(new Line(current, p, currentAttribs));
                    current = p;
                }
            }
            else
            {
                if (item.Points.Count > 0)
                {
                    current = current.Add(item.Points.Last());
                }
            }
        }

        protected override void VisitInitialization(Initialization item)
        {
            currentAttribs.Pen = 0;
        }

        protected override void VisitSelectPen(SelectPen item)
        {
            currentAttribs.Pen = item.Pen;
        }

        protected override void VisitLabel(Label item)
        {
            throw new NotImplementedException("Optimalization of LB is not implemented. You must textify it, or disable optimalization.");
        }

        protected override void VisitUnknown(UnknownCommand item)
        {
            throw new NotImplementedException("Cannot optimize unknown instruction: " + item.Command);
        }

        public class Attributes
        {
            public Attributes() { }
            public Attributes(Attributes a)
            {
                this.Pen = a.Pen;
            }
            public int Pen { get; set; }
        }

        public class Line
        {
            public Line()
            {
                Attribs = new Attributes();
            }
            public Line(HPoint p1, HPoint p2, Attributes a)
            {
                P1 = p1;
                P2 = p2;
                Attribs = new Attributes(a);
            }
            public HPoint P1 { get; set; }
            public HPoint P2 { get; set; }
            public Attributes Attribs { get; set; }
        }

        protected double GetPenUpLength(List<Line> lines)
        {
            double length = 0;
            var pos = new HPoint();
            foreach (var item in lines)
            {
                if (item.P1 != pos)
                {
                    length += HPoint.LengthAbs(pos, item.P1);
                }
                pos = item.P2;
            }
            return length;
        }

    }



}
