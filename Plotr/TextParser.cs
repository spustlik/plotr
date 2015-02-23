using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextParsing
{
    public abstract class TextParser
    {
        protected StreamReader _reader;
        private char? readed;
        protected char peek()
        {
            if (readed == null)
            {
                read();
            }
            return readed.Value;
        }

        protected void read()
        {
            readed = (char)_reader.Read();
        }
    }
}
