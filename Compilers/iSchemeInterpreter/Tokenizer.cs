using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    public static class Tokenizer
    {
        public static string[] Tokenize(string source)
        {
            return source.Replace("(", " ( ").Replace(")", " ) ").Split(" \t\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        public static string Print(string[] lexes)
        {
            return $"[{string.Join(", ", lexes.Select(s => $"'{s}'"))}]";
        }
    }
}
