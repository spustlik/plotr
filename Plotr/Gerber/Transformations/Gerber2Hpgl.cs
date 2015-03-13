using Gerber.Language;
using Graphics2D;
using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gerber.Transformations
{
    public class Gerber2Hpgl
    {
        /// <summary>
        /// pen width compensation in [mm]
        /// </summary>
        public double PenWidthCompensation { get; set; }
        public Units Units { get; set; }
        private int fillStep = 500; //1/1000 mm

        private bool _exposure = false;
        private string _currentTool;
        private ApertureDefinition _currentAperture;
        private HPoint _lastPosition;

        internal List<Hpgl.Language.HpglItem> Translate(List<GerberItem> gerberData)
        {
            if (PenWidthCompensation > 0)
                fillStep = (int)(PenWidthCompensation * 1000 * 0.8 * 2);
            var result = new List<Hpgl.Language.HpglItem>();
            foreach (var g in gerberData)
            {
                if (g is SetUnitsCommand)
                {
                    Units = ((SetUnitsCommand)g).Units;
                }
                if (g is SetUnitsTypeCommand)
                {
                    var cmd = (SetUnitsTypeCommand)g;
                    if (cmd.UnitsType != UnitsType.Absolute)
                    {
                        throw new NotImplementedException("Units " + cmd.UnitsType + " are not implemented");
                    }
                }
                if (g is ToolPrepareCommand)
                {
                    var cmd = (ToolPrepareCommand)g;
                    _currentTool = cmd.Param;
                    _currentAperture = gerberData.OfType<ApertureDefinition>().FirstOrDefault(a => a.Name == cmd.Param);
                    if (_currentAperture == null)
                    {
                        Console.WriteLine("Error: tool prepare - tool not found " + cmd.Param);
                    }
                }
                if (g is SetExposureCommand)
                {
                    var cmd = (SetExposureCommand)g;
                    _exposure = cmd.On;
                }
                if (g is XYCommand)
                {
                    var cmd = (XYCommand)g;
                    var pos = Transform(cmd.X, cmd.Y);
                    if (cmd.Param == "D01")
                    {
                        var ca = _currentAperture as CircularApertureDefinition;
                        if (ca != null)
                        {
                            //result.Add(new SelectPen() { Pen = (int)ca.R * 100 });
                            var w = 1000 * 25.4 * ca.R / 2;
                            //var size = Transform(w, w);
                            var points = new List<HPoint>();
                            points.Add(pos);
                            var offset = fillStep;
                            while (offset < w)
                            {
                                points.AddRange(GetOutlinedRect(pos, _lastPosition, offset));
                                offset += fillStep;
                            }
                            if (points.Count > 1)
                            {
                                points.Add(pos);
                            }
                            result.Add(new PenDown() { Points = points });
                        }
                        else
                        {
                            result.Add(new PenDown() { Points = { pos } });
                        }
                    }
                    if (cmd.Param == "D02")
                    {
                        result.Add(new PenUp() { Points = { pos } });
                    }
                    if (cmd.Param == "D03")
                    {
                        if (_currentAperture == null)
                        {
                            throw new ApplicationException("No selected aperture");
                        }

                        //result.Add(new PenUp(){Points = {Transform(cmd.X, cmd.Y)}});
                        //result.Add(new Label() { Text = _currentAperture.Name });

                        if (_currentAperture is CircularApertureDefinition)
                        {
                            FilledCircle(result, cmd.X, cmd.Y, ((CircularApertureDefinition)_currentAperture).R);
                        }
                        else if (_currentAperture is RectangleApertureDefinition)
                        {
                            var r = (RectangleApertureDefinition)_currentAperture;
                            FilledRectangle(result, cmd.X, cmd.Y, r.X, r.Y);
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown type of aperture " + _currentAperture);
                        }
                    }
                    _lastPosition = pos;
                }
            }
            return result;
        }

        private HPoint[] GetOutlinedRect(HPoint p1, HPoint p2, int outline)
        {
            /*     p1
             *   +--+--+
             *    \  \  \
             *     \  \  \
             *      +--+--+
             *         p2   p2+len
             * 
             */
            if (p1 == p2)
            {
                return new[] { p1, p1, p1, p1 };
            }

            var pt1 = new Point2D(p1.X, p1.Y);
            var pt2 = new Point2D(p2.X, p2.Y);
            var vector = Vector2D.FromPoints(pt2, pt1);
            var points = new[]{
                new Point2D(-outline,0),
                new Point2D(0,outline),
                new Point2D(vector.Length,outline),
                new Point2D(vector.Length+outline,0),
                new Point2D(vector.Length,-outline),
                new Point2D(0,-outline),
                new Point2D(-outline,0),
            };
            var rotated = points.Select(pt => pt1.Add(pt.Rotate(vector.Angle))).Select(pt => new HPoint((int)pt.X, (int)pt.Y)).ToArray();
            return rotated;
        }

        private HPoint Transform(double x, double y)
        {
            //returned units are 1/1000 mm
            if (Units == Units.Millimeters)
                return new HPoint((int)(1000 * x), (int)(1000 * y));
            else
                // 1 inch = 25.4 mm
                return new HPoint((int)(25.4 * x / 100.0), (int)(25.4 * y / 100.0));
        }

        private void FilledRectangle(List<HpglItem> result, double x, double y, double width, double height)
        {
            //mils = 1/1000 inch
            var w = 1000 * 25.4 * width / 2;
            var h = 1000 * 25.4 * height / 2;
            var pt = Transform(x, y);
            var size = new HPoint((int)w, (int)h); // Transform(w, h);
            size = CompensatePen(size);
            FillRect(result, pt, size.X, size.Y);
        }

        private HPoint CompensatePen(HPoint size)
        {
            if (PenWidthCompensation > 0)
            {
                size = size.Sub(new HPoint((int)(1000 * PenWidthCompensation), (int)(1000 * PenWidthCompensation)));
            }
            return size;
        }

        private void FilledCircle(List<HpglItem> result, double x, double y, double r)
        {
            //mils = 1/1000 inch
            var w = 1000 * 25.4 * r / 2;

            var pt = Transform(x, y);
            var size = new HPoint((int)w, (int)w); // Transform(w, w);
            size = CompensatePen(size);
            w = FillCircle(result, pt, size.X);
        }

        private int FillCircle(List<HpglItem> result, HPoint pt, int radius)
        {
            do
            {
                DrawCircle(result, pt, radius);
                radius = radius - fillStep;
            } while (radius > 0);
            return radius;
        }

        private void FillRect(List<HpglItem> result, HPoint pt, int w, int h)
        {
            do
            {
                DrawRectangle(result, pt, w, h);
                if (w > h)
                {
                    var ratio = 1.0 * w / h;
                    h = h - fillStep;
                    w = w - (int)(fillStep / ratio);
                }
                else
                {
                    var ratio = 1.0 * w / h;
                    w = w - fillStep;
                    h = h - (int)(fillStep / ratio);
                }
            } while (w > 0 && h > 0);
        }

        private static void DrawRectangle(List<HpglItem> result, HPoint pt, int w, int h)
        {
            var p1 = pt.Add(new HPoint() { X = w, Y = h });
            var p2 = pt.Add(new HPoint() { X = -w, Y = h });
            var p3 = pt.Add(new HPoint() { X = -w, Y = -h });
            var p4 = pt.Add(new HPoint() { X = w, Y = -h });
            result.Add(new PenUp() { Points = { p1 } });
            result.Add(new PenDown() { Points = { p2, p3, p4, p1 } });
            result.Add(new PenUp() { });
        }

        private static void DrawCircle(List<HpglItem> result, HPoint pt, int w)
        {
            var q = (int)(w / 1.44);
            var p1 = pt.Add(new HPoint() { X = w, Y = w });
            var p2 = pt.Add(new HPoint() { X = -w, Y = w });
            var p3 = pt.Add(new HPoint() { X = -w, Y = -w });
            var p4 = pt.Add(new HPoint() { X = w, Y = -w });
            /*   .4.
             *  5   3
             * .     .
             * 6     2
             * .     .
             *  7   1
             *   .0.
             */

            result.Add(new PenUp() { Points = { pt.Add(0, w) } });
            result.Add(new PenDown()
            {
                Points = { 
                           //pt.Add(0,w),
                           pt.Add(q,q),
                           pt.Add(w,0),
                           pt.Add(q,-q),
                           pt.Add(0,-w),
                           pt.Add(-q,-q),
                           pt.Add(-w,0),
                           pt.Add(-q,q),
                           pt.Add(0,w),
                         }
            });
            result.Add(new PenUp() { });
        }

    }
}
