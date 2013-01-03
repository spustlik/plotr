using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Hpgl.Converters
{    

    public class Hpgl2Bmp : Measure
    {        
        private Pen currentGPen;
        private Graphics g;
        private Pen debugPen;
        public bool DebugPenUp = true;
        public bool Numbering = true;
        public void Process(Bitmap bmp, List<HpglItem> hpgl)
        {
            currentGPen = Pens.Black;
            //debugPen = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot,  };
            debugPen = new Pen(Color.Red, 3);
            using (g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                Visit(hpgl);
            }
            g = null;
        }


        int counter = 0;
        HPoint lastText = new HPoint();
        protected override void MoveTo(HPoint p)
        {
            if (isPenDown)
            {
                if (Numbering && HPoint.LengthAbs(p, lastText)>20)
                {
                    var tp = ToPoint(p);
                    g.DrawString(counter + "", SystemFonts.IconTitleFont, Brushes.Black, tp.X, tp.Y);
                    counter++;
                    lastText = p;
                }
                if (current == p)
                {
                    g.DrawLine(currentGPen, ToPoint(p), ToPoint(p.Add(new HPoint(1, 1))));
                }
                else
                {
                    g.DrawLine(currentGPen, ToPoint(current), ToPoint(p));
                }
            }
            else
            {
                if (DebugPenUp)
                {
                    g.DrawLine(debugPen, ToPoint(current), ToPoint(p));
                }
            }
            base.MoveTo(p);
        }

        private Point ToPoint(HPoint p)
        {
            return new Point(p.X, (int)g.VisibleClipBounds.Height - p.Y);
        }

        protected override void VisitSelectPen(SelectPen item)
        {
            base.VisitSelectPen(item);
            switch (item.Pen)
            {
                case 0: currentGPen = Pens.Black; break;
                case 1: currentGPen = Pens.Red; break;
                case 2: currentGPen = Pens.Green; break;
                case 3: currentGPen = Pens.Blue; break;
                case 4: currentGPen = Pens.Magenta; break;
                default: currentGPen = Pens.Orange; break;
            }
        }

        protected override void VisitLabel(Label item)
        {
            throw new NotImplementedException("Drawing label to bitmap is not implemented. You must textify it.");
        }
    }
}
