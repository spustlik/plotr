using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hpgl.Converters
{
    public class Hpgl2File
    {
        public void Process(string outFile, List<HpglItem> hpgl)
        {
            using (var sw = new StreamWriter(outFile))
            {
                foreach (var item in hpgl.Where(h=>!(h is Terminator)))
                {
                    sw.Write(item.HpglStr());
                    sw.Write(";");
                }
            }
        }
    }
}
