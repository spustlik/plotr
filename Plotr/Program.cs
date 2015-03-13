using Gerber.Language;
using Hpgl.Converters;
using Hpgl.Language;
using Hpgl.Transformations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Plotr
{
    class Program
    {
        private static CommandLine cmd;
        static void Main(string[] args)
        {
            cmd = new CommandLine(args);

            if (cmd.arguments.Count < 1)
            {
                Help();
                return;
            }
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
            }
        }

        private static void Run()
        {
            var inputFileName = cmd.arguments.First();
            if (!File.Exists(inputFileName))
            {
                throw new Exception(String.Format("File {0} does not exists", inputFileName));
            }

            //TODO?: read SVG and convert to HPGL
            //TODO?: vectorize SVG, so white fills are wiped out from vectors
            List<HpglItem> hpglData;

            if (cmd.GetParamOrDefault("gerber", false))
            {
                var gerberParser = new GerberParser();
                using (var f = File.OpenRead(inputFileName))
                {
                    var gerberData = gerberParser.Parse(f);
                    foreach (var item in gerberData.OfType<Gerber.Language.UnknownCommand>())
                    {
                        Console.WriteLine(item);
                    }
                    var comp = cmd.GetParamOrDefault("gerberpen", 0.0);
                    var unitstext = cmd.GetParamOrDefault("gerberunits", "Inches");
                    var units = (Units)Enum.Parse(typeof(Units), unitstext);
                    var g2h = new Gerber.Transformations.Gerber2Hpgl() { PenWidthCompensation = comp, Units = units };
                    hpglData = g2h.Translate(gerberData);
                }
            }
            else
            {
                var parser = new HpglParser();
                using (var f = File.OpenRead(inputFileName))
                {
                    hpglData = parser.Parse(f);
                }
                Console.WriteLine("HPGL file {0} readed", inputFileName);
                Console.WriteLine("Stats:");
                Console.WriteLine("Parser: {0} tokens, {1} commands", hpglData.Count, hpglData.Where(h => !(h is Terminator)).Count());
            }


            //MEASURE
            {
                var measure = new Measure();
                measure.Visit(hpglData);
                Console.WriteLine("Dimensions: min={0}, max={1}", measure.Min, measure.Max);
                Console.WriteLine("Length of lines: with pen down={0:.1}, with pen up={1:.1}", measure.PenDownLength, measure.PenUpLength);
            }
            //remove unknowns
            hpglData = hpglData.Where(h => !(h is Hpgl.Language.UnknownCommand)).ToList();

            int width = cmd.GetParamOrDefault("width", 1520);
            int height = cmd.GetParamOrDefault("height", 2160);

            if (cmd.GetParamOrDefault("textify", false))
            {
                Console.WriteLine("Textificating...");
                var t = new Textificator();
                hpglData = t.Process(hpglData);
            }
            if (cmd.GetParamOrDefault("absolutize", false))
            {
                var a = new Absolutizer();
                hpglData = a.Process(hpglData);
            }
            //transformation
            {
                var transformer = new Transformer(cmd.GetParamOrDefault("transform", string.Empty));
                hpglData = transformer.Transform(hpglData);
            }

            //autoscale
            if (cmd.GetParamOrDefault("autoscale", false))
            {
                Console.WriteLine("Autoscaling...");
                var measure = new Measure();
                measure.Visit(hpglData);
                if (measure.ContainsRelative)
                {
                    Console.WriteLine("Warning: input contains PR, results can be strange");
                }
                Console.WriteLine("Dimensions: min={0}, max={1}", measure.Min, measure.Max);
                var size = new HPoint(measure.Max.X - measure.Min.X, measure.Max.Y - measure.Min.Y);
                //size.X/size.Y=width/height
                //newptx/pt.x = width/size.X
                double sx = (double)(width * 0.9d) / size.X;
                double sy = (double)(height * 0.9d) / size.Y;
                var transformer = new Transformer();
                transformer.Move(-measure.Min.X, -measure.Min.Y);
                transformer.Scale(sx < sy ? sx : sy);
                transformer.Move((int)(width * 0.05d), (int)(height * 0.05d));
                hpglData = transformer.Transform(hpglData);
            }

            //optimalization
            if (cmd.GetParamOrDefault("optimize", true))
            {
                Console.WriteLine("Optimizing...");
                var optimizer = new SegmentationOptimizer();
                hpglData = optimizer.Process(hpglData);

                var optmeasure = new Measure();
                optmeasure.Visit(hpglData);
                Console.WriteLine("Optimized lines: with pen down={0:.1}, with pen up={1:.1}", optmeasure.PenDownLength, optmeasure.PenUpLength);
            }

            
            var maxstep = cmd.GetParamOrDefault("maxstep", 0);
            if (maxstep > 0)
            {
                int split = 0;
                int i = 0;
                while (i < hpglData.Count)
                {
                    var pcmd = hpglData[i] as HpglPointsCommand;
                    if (pcmd != null)
                    {
                        if (pcmd.Points.Count > maxstep)
                        {
                            split++;
                            var newcmd = Activator.CreateInstance(pcmd.GetType()) as HpglPointsCommand;
                            newcmd.Points = pcmd.Points.Take(maxstep).ToList();
                            hpglData.Insert(i, newcmd);
                            pcmd.Points = pcmd.Points.Skip(maxstep).ToList();
                        }
                    }
                    i++;
                }
                Console.WriteLine("Splitted " + split + " commands for maximum " + maxstep + " steps.");
            }

            //outline
            var outline = cmd.GetParamOrDefault("outline", -1);
            if (outline >= 0)
            {
                var measure = new Measure();
                measure.Visit(hpglData);
                var size = new HPoint(outline, outline);
                var rect = new[]{
                    new HPoint(measure.Min.X - outline, measure.Max.Y + outline),
                    new HPoint(measure.Max.X + outline, measure.Max.Y + outline),
                    new HPoint(measure.Max.X + outline, measure.Min.Y - outline),
                    new HPoint(measure.Min.X - outline, measure.Min.Y - outline), 
                };
                hpglData.Insert(0, new PenUp() { Points = { rect[3] } });
                hpglData.Insert(1, new PenDown() { Points = { rect[0], rect[1], rect[2], rect[3] } });
                //hpglData.Insert(2, new PenUp() {  } );
            }

            //speed, delay
            var s = cmd.GetParamOrDefault("speed", 0);
            var d = cmd.GetParamOrDefault("updelay",0);
            if (s != 0 || d!=0)
            {
                var ss = new SetSpeed() { SpeedMove = s, DelayUp = d };
                hpglData.Insert(0, ss);
            }
            if (cmd.GetParamOrDefault("noinit", false))
            {
                hpglData.RemoveAll(x => x is Initialization);
            }

            var outputType = cmd.GetParamOrDefault("output", "image").ToLowerInvariant();
            switch (outputType)
            {
                case "image":
                    {
                        //TODO3: 
                        // colorization - add colors (SP, better gray) by position in list 
                        var outFile = cmd.GetParamOrDefault("filename", "result.png");
                        Console.WriteLine("Writing to bitmap {0} ({1}x{2})", outFile, width, height);
                        var bmp = new Bitmap(width, height);
                        var hpgl2Bmp = new Hpgl2Bmp();
                        hpgl2Bmp.DebugPenUp = cmd.GetParamOrDefault("showPenUp", false);
                        hpgl2Bmp.Numbering = cmd.GetParamOrDefault("showNumbers", false);
                        hpgl2Bmp.Process(bmp, hpglData);
                        bmp.Save(outFile);
                        break;
                    }
                case "serial":
                    {
                        var sp = cmd.GetParamOrDefault("serial", "COM1,9600");
                        var ser = new Hpgl2SerialConsole(sp);
                        if (cmd.GetParamOrDefault("paused", false))
                            ser.IsPaused = true;
                        Console.WriteLine("Writing to serial {0},{1},{2},{3},{4}", ser.Port.PortName, ser.Port.BaudRate, ser.Port.Parity, ser.Port.DataBits, ser.Port.StopBits);
                        ser.Proces(hpglData);
                        break;
                    }
                case "file":
                    {
                        var outFile = cmd.GetParamOrDefault("filename", "result.plt");
                        Console.WriteLine("Writing to file {0}", outFile);
                        var f = new Hpgl2File();
                        f.Process(outFile, hpglData);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("output");
                    }
            }
        }

        private static void Help()
        {
            Console.WriteLine("PLOTR - HPGL processing utility *** Copyright (c) 2012-2015 Jan Stuchlik");
            Console.WriteLine("Syntax: {0} input /output=image|serial|file [options]", Assembly.GetExecutingAssembly().GetName().Name);
            Console.WriteLine("Common options:");
            Console.WriteLine("  /textify=true - replace LB with default 6x4 font (default:false)");
            Console.WriteLine("  /absolutize=true - remove PR instructions, which is needed for transformation (default:false)");
            Console.WriteLine("  /transform=mx:450,my:42,rot:90,zoom:1.3 - moves, rotates or scales plot in given order (default:none)");
            Console.WriteLine("  /autoscale=true - autoscales image after transformation (default:false)");
            Console.WriteLine("  /speed=x - set speed x (ms) for moving pen when down");
            Console.WriteLine("  /delay=x - set delay x (ms) when pen just raised");
            Console.WriteLine("  /noinit=true - removes any initialization (IN) instructions");
            Console.WriteLine("  /maxstep=64 - maximum length-step (PU,PD,...)");
            Console.WriteLine();
            Console.WriteLine("Optimalization options:");
            Console.WriteLine("  /optimize=false - disable optimalization");
            Console.WriteLine();
            Console.WriteLine("FILE - writes file to new HPGL");
            Console.WriteLine("  /filename=filename - name of file (dafult: result.plt)");
            Console.WriteLine();
            Console.WriteLine("SERIAL - sends output to serial port");
            Console.WriteLine("  /serial=COM1,9600 - write result to serial COM1 port with 9600 bauds");
            Console.WriteLine("  /paused=true - waits for keyboard before sending any command");
            Console.WriteLine();
            Console.WriteLine("IMAGE - plots output to bitmap");
            Console.WriteLine("  /filename=filename - name of file (dafult: result.png)");
            Console.WriteLine("  /showPenUp=true - draw also pen up (for debugging optimalization)");
            Console.WriteLine("  /showNumbers=true - numbers the lines");
            Console.WriteLine("  /width=1000 - sets output image width");
            Console.WriteLine("  /height=1000 - sets output image height");
            Console.WriteLine();
            Console.WriteLine("GERBER - it can also read simple Gerber files from Fritzing or EAGLE");
            Console.WriteLine("  file will be internally converted into HPGL with scale 1/1000 mm; use /transform=zoom:0.008 for MINIGRAF");
            Console.WriteLine("  /gerber=true - means that input file is in gerber format");
            Console.WriteLine("  /gerberpen=1.3 - sets pen width compensation in [mm] for filling apertures (default: 0.0)");
            Console.WriteLine("  /gerberunits=Inches/Millimeters - sets default gerber units (default:Inches)");
        }

    }
}
