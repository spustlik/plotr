using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Language
{
    public abstract class HpglProcessorBase : HpglAllVisitor
    {
        protected bool isPenDown = false;
        protected HPoint current = new HPoint(0, 0);
        protected int currentPen = 0;
        public HPoint Min = new HPoint(Int32.MaxValue, Int32.MaxValue);
        public HPoint Max = new HPoint(0, 0);
        public bool ContainsRelative = false;
        public double PenUpLength, PenDownLength;

        protected override void VisitPenUp(PenUp item)
        {
            isPenDown = false;
            foreach (var p in item.Points)
            {
                MoveTo(p);
            }
        }

        protected override void VisitPenDown(PenDown item)
        {
            isPenDown = true;
            foreach (var pt in item.Points)
            {
                MoveTo(pt);
            }
        }

        protected override void VisitPenAbsolute(PenAbsolute item)
        {
            foreach (var pt in item.Points)
            {
                MoveTo(pt);
            }
        }

        protected override void VisitPenRelative(PenRelative item)
        {
            ContainsRelative = true;
            foreach (var rpt in item.Points)
            {
                var pt = current.Add(rpt);
                MoveTo(pt);
            }
        }

        protected virtual void MoveTo(HPoint pt)
        {
            if (isPenDown)
            {
                PenDownLength += HPoint.LengthAbs(current, pt);
                Min.X = Math.Min(pt.X, Min.X);
                Min.Y = Math.Min(pt.Y, Min.Y);
                Max.X = Math.Max(pt.X, Max.X);
                Max.Y = Math.Max(pt.Y, Max.Y);
            }
            else
            {
                PenUpLength += HPoint.LengthAbs(current, pt);
            }
            current = pt;
        }

        protected override void VisitSelectPen(SelectPen item)
        {
            currentPen = item.Pen;
        }

        protected override void VisitUnknown(UnknownCommand item)
        {
            Console.WriteLine("Warning: unknown command {0} ({1})", item.HpglStr(), item);
        }

    }

    public class Measure : HpglProcessorBase
    {
        protected override void VisitTerminator(Terminator item)
        {
            //
        }

        protected override void VisitInitialization(Initialization item)
        {
            //
        }

        protected override void VisitLabel(Label item)
        {
            //TODO:
        }

    }

}
