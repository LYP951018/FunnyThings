using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    public static class Parser
    {
        public static SExpression Parse(this string code)
        {
            var program = new SExpression("", null);
            var current = program;
            foreach(var lex in Tokenizer.Tokenize(code))
            {
                if(lex == "(")
                {
                    var node = new SExpression("(", current);
                    current.Children.Add(node);
                    current = node;
                }
                else if(lex == ")")
                {
                    current = current.Parent;
                }
                else
                {
                    current.Children.Add(new SExpression(lex, current));
                }
            }

            return program.Children[0];
        }
    }
}
