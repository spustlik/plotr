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
        public Hpgl2SerialConsole(string serialParams)
            : base(serialParams)
        {
        }

        public override void Process(List<HpglItem> hpgl)
        {
            Console.WriteLine("Press <P> for pause...");
            base.Process(hpgl);
        }

        protected override string Send(string ins)
        {
            ins = ins + ";";
            Console.WriteLine("#" + ins);
            var result = base.Send(ins);
            Console.WriteLine(">" + result);
            var k = PressedKey();
            if (k != null && k.Value.Key == ConsoleKey.P)
            {
                Console.WriteLine("\nPaused... press <U> for unpause, <Q> for quit");
                ProcessKey();
            }
            return result;
        }

        private void ProcessKey()
        {
            ConsoleKeyInfo? k = null;
            do
            {
                Thread.Sleep(200);
                k = PressedKey();
                if (k != null)
                {
                    switch (k.Value.Key)
                    {
                        case ConsoleKey.U:
                            return;
                        case ConsoleKey.Q:
                            Send("PU;");
                            Environment.Exit(1);
                            return;
                    }
                    k = null;
                }
            } while (k == null);
        }

        private ConsoleKeyInfo? PressedKey()
        {
            if (!Console.KeyAvailable)
                return null;
            return Console.ReadKey(true);
        }
    }
}
