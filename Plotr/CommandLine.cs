using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plotr
{
    public class CommandLine
    {
        public List<Tuple<string, string>> parameters;
        public List<string> arguments;

        public CommandLine(string[] args)
        {
            parameters = args.Where(a => IsCmdParam(a)).Select(a => ParseCmdParam(a)).ToList();
            arguments = args.Where(a => !IsCmdParam(a)).ToList();
        }

        public static bool IsCmdParam(string a)
        {
            return a.StartsWith(@"/") || a.StartsWith("-");
        }

        public static Tuple<string, string> ParseCmdParam(string a)
        {
            a = a.Substring(1);
            var parts = a.Split(new string[1] { "=" }, 2, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                return Tuple.Create(parts[0], parts[1]);
            }
            return Tuple.Create(a, "");
        }

        public string GetParamOrDefault(string name, string defValue)
        {
            var r = parameters.FirstOrDefault(p => p.Item1.ToUpper() == name.ToUpper());
            if (r == null)
                return defValue;
            return r.Item2;
        }

        public int GetParamOrDefault(string name, int defValue)
        {
            var r = parameters.FirstOrDefault(p => p.Item1.ToUpper() == name.ToUpper());
            if (r == null)
                return defValue;
            return Int32.Parse(r.Item2);
        }

        public double GetParamOrDefault(string name, double defValue)
        {
            var r = parameters.FirstOrDefault(p => p.Item1.ToUpper() == name.ToUpper());
            if (r == null)
                return defValue;
            return Double.Parse(r.Item2);
        }

        public bool GetParamOrDefault(string name, bool defValue)
        {
            var r = parameters.FirstOrDefault(p => p.Item1.ToUpper() == name.ToUpper());
            if (r == null)
                return defValue;
            return Boolean.Parse(r.Item2);
        }
    }
}
