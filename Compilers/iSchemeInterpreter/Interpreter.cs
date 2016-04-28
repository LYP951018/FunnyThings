using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    public static class Interpreter
    {
        public static void KeepInterpretingInConsole(this SScope scope, Func<string, SScope,SObject> evaluate)
        {
            while (true)
            {
                //try
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(">> ");
                    String code;
                    if (!String.IsNullOrWhiteSpace(code = Console.ReadLine()))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(">> " + evaluate(code, scope));
                    }
                }
                //catch (Exception ex)
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine(">> " + ex.Message);
                //}
            }

        }
    }
}
