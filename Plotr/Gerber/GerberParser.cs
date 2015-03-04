using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Gerber.Language
{
    public class GerberParser
    {
        internal List<GerberItem> Parse(System.IO.FileStream f)
        {
            using (var sr = new StreamReader(f))
            {
                return Parse(sr);
            }
        }

        private List<GerberItem> Parse(StreamReader sr)
        {
            //TODO: maybe replace with char-reading alg.
            // see official gerber doc for basic syntax
            // http://www.ucamco.com/files/downloads/file/81/the_gerber_file_format_specification.pdf
            // page 37
            var result = new List<GerberItem>();
            string line;
            do
            {
                line = sr.ReadLine();
                if (line != null && line.Length>0)
                {
                    result.Add(ParseLine(line));
                }
            }
            while (line != null);
            return result;
        }

        private GerberItem ParseLine(string line)
        {
            switch (line.Substring(0, 1).ToUpperInvariant())
            {
                case "G": return ParseGCommand(line);
                case "D": return ParseDCommand(line);
                case "%": return ParseSpecCommand(line);
                case "X": return ParseXCommand(line);
                default:
                    return new UnknownCommand() { Line = line };

            }
        }

        private GerberItem ParseXCommand(string line)
        {
            if (!line.StartsWith("X"))
                throw new FormatException();
            line = line.Substring(1);
            var x = readNumber(ref line);
            if (!line.StartsWith("Y"))
                throw new FormatException();
            line = line.Substring(1);
            var y = readNumber(ref line);
            var rest = line;
            return new XYCommand() { X = x, Y = y, Param = rest.TrimEnd('*') };
        }

        private double readNumber(ref string line)
        {
            string s = "";
            while(line.Length>0)
            {
                var c = line[0];
                if (Char.IsWhiteSpace(c))
                {
                    line = line.Substring(1);
                }
                else if (Char.IsDigit(c) || c == '.')
                {
                    s = s + c;
                    line = line.Substring(1);
                }
                else
                {
                    break;
                }
            }
            return Double.Parse(s, CultureInfo.InvariantCulture);
        }

        private GerberItem ParseSpecCommand(string line)
        {
            var inner = line.Trim('%');
            if (inner.StartsWith("AD"))
            {
                var ap = inner.Substring(2,3);
                var type = inner.Substring(5,1);
                if (type == "R")
                {
                    inner = inner.Substring(7).Trim();
                    var x = readNumber(ref inner);
                    if (!inner.StartsWith("X"))
                        throw new FormatException();
                    inner = inner.Substring(1);
                    var y = readNumber(ref inner);

                    return new RectangleApertureDefinition() { X = x, Y = y, Name = ap };
                }
                else if (type == "C")
                {
                    inner = inner.Substring(7).Trim();
                    var r = readNumber(ref inner);
                    return new CircularApertureDefinition() { R = r, Name = ap };
                }
            }
            return new SpecialCommand() { Line = line };
        }

        private GerberItem ParseDCommand(string line)
        {
            switch (line.Substring(1, 2))
            {
                case "01":
                    return new SetExposureCommand() { On = true };
                case "02":
                    return new SetExposureCommand() { On = false };
                case "03":
                    return new FlashApertureCommand();
                default:
                    return new UnknownCommand() { Line = line };
            }
        }

        private GerberItem ParseGCommand(string line)
        {
            switch (line.Substring(1, 2))
            {
                //case "00": //move
                //case "01": //1x lin. interpolation
                //case "02": //cw interpolation
                //case "03": //ccw interpolation
                case "04":
                    return new CommentCommand() { Comment = line.Substring(3).Trim() };
                //case "10": //10x lin. 
                //case "11": //0.1x lin.
                //case "12": //0.01x lin.
                //case "36": //poly fill on
                //case "37": //poly fill off
                case "54":
                    return new ToolPrepareCommand() { Param = line.Substring(3).TrimEnd('*') };
                //case "55": //flash prepare                    
                case "70":
                    return new SetUnitsCommand() { Units = Units.Inches };
                case "71":
                    return new SetUnitsCommand() { Units = Units.Millimeters };
                //case "74": //disable circ. interpolation
                //case "75": //enable circ. interpolation
                case "90":
                    return new SetUnitsTypeCommand() { UnitsType = UnitsType.Absolute };
                case "91":
                    return new SetUnitsTypeCommand() { UnitsType = UnitsType.Relative };
                default:
                    return new UnknownCommand() { Line = line };
            }
        }
    }

    public abstract class GerberItem
    {

    }

    public abstract class ApertureDefinition : GerberItem
    {
        public string Name { get; set; }
    }

    public class CircularApertureDefinition : ApertureDefinition
    {
        public double R { get; set; }
        public override string ToString()
        {
            return "Circular aperture:" + Name + ", R=" + R;
        }
    }
    public class RectangleApertureDefinition : ApertureDefinition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public override string ToString()
        {
            return "Rect aperture:" + Name + " " + X + "x" + Y;
        }
    }

    public class XYCommand : GerberItem
    {

        public double X { get; set; }

        public double Y { get; set; }

        public string Param { get; set; }
        public override string ToString()
        {
            return "X:" + X + ", Y:" + Y + ", " + Param;
        }
    }
    public class SpecialCommand : UnknownCommand
    {
        public override string ToString()
        {
            return "[Special]:" + Line;
        }

    }

    //means down then up
    public class FlashApertureCommand : GerberItem
    {
        public override string ToString()
        {
            return "Flash";
        }
    }

    //on means down, off means up 
    public class SetExposureCommand : GerberItem
    {

        public bool On { get; set; }
        public override string ToString()
        {
            return "Set Exposure:" + (On ? "On":"Off");
        }
    }
    public enum Units
    {
        Inches,
        Millimeters
    }
    public class SetUnitsTypeCommand : GerberItem
    {

        public UnitsType UnitsType { get; set; }
        public override string ToString()
        {
            return "Units :" + UnitsType;
        }
    }

    public enum UnitsType
    {
        Absolute,
        Relative
    }
    public class SetUnitsCommand : GerberItem
    {

        public Units Units { get; set; }
        public override string ToString()
        {
            return "Units " + Units;
        }
    }
    public class ToolPrepareCommand : GerberItem
    {

        public string Param { get; set; }
        public override string ToString()
        {
            return "Prepare tool " + Param;
        }
    }
    public class CommentCommand : GerberItem
    {
        public string Comment { get; set; }
        public override string ToString()
        {
            return "/* " + Comment + " */";
        }
    }
    public class UnknownCommand : GerberItem
    {
        public string Line { get; set; }
        public override string ToString()
        {
            return "[Unknown]:" + Line;
        }
    }


}
