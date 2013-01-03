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
            var parser = new HpglParser();
            var hpgl = parser.Parse(File.OpenRead(inputFileName));
            Console.WriteLine("HPGL file {0} readed", inputFileName);
            Console.WriteLine("Stats:");
            Console.WriteLine("Parser: {0} tokens, {1} commands", hpgl.Count, hpgl.Where(h => !(h is Terminator)).Count());
            {
                var measure = new Measure();
                measure.Visit(hpgl);
                Console.WriteLine("Dimensions: min={0}, max={1}", measure.Min, measure.Max);
                Console.WriteLine("Length of lines: with pen down={0:.1}, with pen up={1:.1}", measure.PenDownLength, measure.PenUpLength);
            }
            //remove unknowns
            hpgl = hpgl.Where(h => !(h is UnknownCommand)).ToList();

            int width = cmd.GetParamOrDefault("width", 1520);
            int height = cmd.GetParamOrDefault("height", 2160);

            if (cmd.GetParamOrDefault("textify", false))
            {
                Console.WriteLine("Textificating...");
                var t = new Textificator();
                hpgl = t.Process(hpgl);
            }
            if (cmd.GetParamOrDefault("absolutize", false))
            {
                var a = new Absolutizer();
                hpgl = a.Process(hpgl);
            }
            //transformation
            {
                var transformer = new Transformer(cmd.GetParamOrDefault("transform", string.Empty));
                hpgl = transformer.Transform(hpgl);
            }

            //autoscale
            if (cmd.GetParamOrDefault("autoscale", false))
            {
                Console.WriteLine("Autoscaling...");
                var measure = new Measure();
                measure.Visit(hpgl);
                if (measure.ContainsRelative)
                {
                    Console.WriteLine("Warning: input contains PR, results can be strange");
                }
                Console.WriteLine("Dimensions: min={0}, max={1}", measure.Min, measure.Max);
                var size = new HPoint(measure.Max.X - measure.Min.X, measure.Max.Y - measure.Min.Y);
                //size.X/size.Y=width/height
                //newptx/pt.x = width/size.X
                double sx = (double)(width*0.9d) / size.X;
                double sy = (double)(height*0.9d) / size.Y;
                var transformer = new Transformer();
                transformer.Move(-measure.Min.X, -measure.Min.Y);
                transformer.Scale(sx < sy ? sx : sy);
                transformer.Move((int)(width * 0.05d), (int)(height * 0.05d));
                hpgl = transformer.Transform(hpgl);
            }

            //optimalization
            if (cmd.GetParamOrDefault("optimize", true))
            {
                Console.WriteLine("Optimizing...");
                var optimizer = new SegmentationOptimizer();
                hpgl = optimizer.Process(hpgl);

                var optmeasure = new Measure();
                optmeasure.Visit(hpgl);
                Console.WriteLine("Optimized lines: with pen down={0:.1}, with pen up={1:.1}", optmeasure.PenDownLength, optmeasure.PenUpLength);
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
                        hpgl2Bmp.Process(bmp, hpgl);
                        bmp.Save(outFile);
                        break;
                    }
                case "serial":
                    {
                        var sp = cmd.GetParamOrDefault("serial", "COM1,9600");
                        var ser = new Hpgl2SerialConsole(sp);
                        Console.WriteLine("Writing to serial {0},{1},{2},{3},{4}", ser.Port.PortName, ser.Port.BaudRate, ser.Port.Parity, ser.Port.DataBits, ser.Port.StopBits);
                        ser.Process(hpgl);
                        break;
                    }
                case "file":
                    {
                        var outFile = cmd.GetParamOrDefault("filename", "result.plt");
                        Console.WriteLine("Writing to file {0}", outFile);
                        var f = new Hpgl2File();
                        f.Process(outFile, hpgl);
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
            Console.WriteLine("PLOTR - HPGL processing utility *** Copyright (c) 2012 Jan Stuchlik");
            Console.WriteLine("Syntax: {0} input /output=image|serial|file [options]", Assembly.GetExecutingAssembly().GetName().Name);
            Console.WriteLine("Common options:");
            Console.WriteLine("  /textify=true - replace LB with default 6x4 font (default:false)");
            Console.WriteLine("  /absolutize=true - remove PR instructions, which is needed for transformation (default:false)");
            Console.WriteLine("  /transform=mx:450,my:42,rot:90,zoom:1.3 - moves, rotates or scales plot in given order (default:none)");
            Console.WriteLine("  /autoscale=true - autoscales image after transformation (default:false)");
            Console.WriteLine();
            Console.WriteLine("Optimalization options:");
            Console.WriteLine("  /optimize=false - disable optimalization");
            Console.WriteLine("  /maxstep=64 - maximum length-step");
            Console.WriteLine();
            Console.WriteLine("FILE - writes file to new HPGL");
            Console.WriteLine("  /filename=filename - name of file (dafult: result.plt)");
            Console.WriteLine();
            Console.WriteLine("SERIAL - sends output to serial port");
            Console.WriteLine("  /serial=COM1,9600 - write result to serial COM1 port with 9600 bauds");
            Console.WriteLine();
            Console.WriteLine("IMAGE - plots output to bitmap");
            Console.WriteLine("  /filename=filename - name of file (dafult: result.png)");
            Console.WriteLine("  /showPenUp=true - draw also pen up (for debugging optimalization)");
            Console.WriteLine("  /showNumbers=true - numbers the lines");
            Console.WriteLine("  /width=1000 - sets output image width");
            Console.WriteLine("  /height=1000 - sets output image height");
        }

    }
}
