using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hpgl.Converters
{
    public class Hpgl2SerialConsole : Hpgl2Serial
    {
        public bool IsPaused { get; set; }
        public Hpgl2SerialConsole(string serialParams)
            : base(serialParams)
        {
        }

        public override void ProcessCommands()
        {
            Console.WriteLine("Press <P> for pause...");
            base.ProcessCommands();
        }

        protected override string Send(string ins)
        {
            TestKeyboard();

            ins = ins + ";";
            Console.WriteLine("#" + ins);
            var result = base.Send(ins);
            Console.WriteLine(">" + result);
            return result;
        }

        private void TestKeyboard()
        {
            var k = PressedKey();
            if (IsPaused || (k != null && k.Value.Key == ConsoleKey.P))
            {
                IsPaused = false;
                Console.Clear();
                Console.Write("\nPaused...");
                Console.WriteLine(@"
Press <R> for resume, <Q> for quit
<N> process next instruction, <BACKSPACE> goto previous instruction (do not process it)
<SPACE> for showing actual pen position
<ARROWS> for moving pen (CTRL, SHIFT, ALT for 8, 80, 800 multiplier)
<U> for pen UP, <D> for pen down, <1> go to minimum coords <2> go to maximum coords
<I> Initialize, <B> set basic position (BP)
<+/-> increase/decrease speed of pen
");
                DumpCommands();
                KeybardLoop();
            }
        }

        private void DumpCommands()
        {
            var from = Math.Max(0, CurrentCommand - 5);
            for (int i = 0; i < 10; i++)
            {
                int index = from + i;
                if (index >= Commands.Count)
                    break;
                Console.Write(CurrentCommand == index ? ">" : " ");
                var cmd = Commands[index].HpglStr();
                if (cmd.Length > 60)
                    cmd = cmd.Substring(0, 59) + "...";
                Console.WriteLine(cmd);
            }
        }

        int _currentDelay;
        private void KeybardLoop()
        {
            ConsoleKeyInfo? k = null;
            do
            {
                Thread.Sleep(200);
                k = PressedKey();
                if (k != null)
                {
                    switch (k.Value.KeyChar)
                    {
                        case '+':
                            _currentDelay -= 1;
                            if (_currentDelay < 0)
                                _currentDelay = 0;
                            Send("VS" + _currentDelay + ";");
                            break;
                        case '-':
                            _currentDelay += 1;
                            Send("VS" + _currentDelay + ";");
                            break;
                        default:
                            switch (k.Value.Key)
                            {
                                case ConsoleKey.Spacebar:
                                    {
                                        var dump = base.Send("OD;");
                                        Console.WriteLine(">" + dump);
                                        break;
                                    }
                                case ConsoleKey.LeftArrow:
                                case ConsoleKey.RightArrow:
                                case ConsoleKey.UpArrow:
                                case ConsoleKey.DownArrow:
                                    {
                                        MovePen(k.Value);
                                        break;
                                    }
                                case ConsoleKey.D:
                                    base.Send("PD;");
                                    break;
                                case ConsoleKey.U:
                                    base.Send("PU;");
                                    break;
                                case ConsoleKey.R:
                                    Console.WriteLine();
                                    return;
                                case ConsoleKey.Q:
                                    base.Send("PU;");
                                    Environment.Exit(1);
                                    return;
                                case ConsoleKey.B:
                                    base.Send("BP;");
                                    break;
                                case ConsoleKey.I:
                                    base.Send("IN;");
                                    break;
                                case ConsoleKey.N:
                                    IsPaused = true;
                                    return;
                                case ConsoleKey.D1:
                                    {
                                        var measure = new Measure();
                                        measure.Visit(Commands);
                                        base.Send(String.Format("PU{0},{1};", measure.Min.X, measure.Min.Y));
                                        break;
                                    }
                                case ConsoleKey.D2:
                                    {
                                        var measure = new Measure();
                                        measure.Visit(Commands);
                                        base.Send(String.Format("PU{0},{1};", measure.Max.X, measure.Max.Y));
                                        break;
                                    }
                                case ConsoleKey.Backspace:                                    
                                    if (CurrentCommand > 0)
                                        CurrentCommand--;
                                    Console.Clear();
                                    DumpCommands();
                                    break;
                            }
                            break;
                    }
                    k = null;
                }
            } while (k == null);
        }

        private void MovePen(ConsoleKeyInfo k)
        {
            int x = 0;
            int y = 0;
            switch (k.Key)
            {
                case ConsoleKey.LeftArrow:
                    x = -1;
                    break;
                case ConsoleKey.RightArrow:
                    x = 1;
                    break;
                case ConsoleKey.UpArrow:
                    y = -1;
                    break;
                case ConsoleKey.DownArrow:
                    y = 1;
                    break;
            }
            if ((k.Modifiers & ConsoleModifiers.Control) != 0)
            {
                x = x * 8; y = y * 8;
            }
            if ((k.Modifiers & ConsoleModifiers.Shift) != 0)
            {
                x = x * 80; y = y * 80;
            }
            if ((k.Modifiers & ConsoleModifiers.Alt) != 0)
            {
                x = x * 800; y = y * 800;
            }
            base.Send("PR" + x + "," + y + ";");
            var pos = base.Send("OD;");
            Console.Write(pos + "                        \r");
        }

        private ConsoleKeyInfo? PressedKey()
        {
            if (!Console.KeyAvailable)
                return null;
            return Console.ReadKey(true);
        }

    }
}
