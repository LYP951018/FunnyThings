using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            new SScope(null).KeepInterpretingInConsole((code, scope) => code.Parse().Evaluate(scope));
        }
    }
}
