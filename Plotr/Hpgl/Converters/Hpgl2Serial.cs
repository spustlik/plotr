using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hpgl.Converters
{
    public class Hpgl2Serial
    {
        public SerialPort Port;
        public Hpgl2Serial(string serialParams)
        {
            var parts = serialParams.Split(new[] { "," }, StringSplitOptions.None);
            Port = new SerialPort();
            if (parts.Length > 0)
                Port.PortName = parts[0];
            if (parts.Length > 1)
                Port.BaudRate = Int32.Parse(parts[1]);
            if (parts.Length > 2)
                Port.Parity = (Parity)Enum.Parse(typeof(Parity), parts[2], true);
            if (parts.Length > 3)
                Port.DataBits = Int32.Parse(parts[3]);
            if (parts.Length > 4)
                Port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), parts[4], true);
            Port.Open();
        }

        protected virtual string Send(string ins)
        {
            Port.WriteLine(ins);
            var result = Port.ReadLine();
            return result;
        }

        public List<HpglItem> Commands { get; set; }
        public int CurrentCommand { get;set;}
        public void Proces(List<HpglItem> hpgl)
        {
            Commands = hpgl;
            CurrentCommand = 0;
            ProcessCommands();
        }
        public virtual void ProcessCommands()
        {
            while (CurrentCommand < Commands.Count)
            {
                var item = Commands[CurrentCommand++];
                if (item is Terminator)
                    continue;
                ProcessItem(item);
            }
        }

        protected virtual void ProcessItem(HpglItem item)
        {
            Send(item.HpglStr());
        }

    }


}
