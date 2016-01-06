using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    public static class SScopeExtentions
    {
        public static IEnumerable<SObject> Evaluate(this IEnumerable<SExpression> expressions, SScope scope)
        {
            return expressions.Select(exp => exp.Evaluate(scope));
        }

        public static IEnumerable<T> Evaluate<T>(this IEnumerable<SExpression> expressions, SScope scope)
            where T : SObject
        {
            return expressions.Evaluate(scope).Cast<T>();
        }

        public static IEnumerable<T> Cast<T>(this IEnumerable<SObject> objects)
           where T : class
        {
            return objects.Select(obj => obj as T);
        }

        public static SBool ChainRelation(this SExpression[] expressions, SScope scope, Func<SNumber,SNumber,Boolean> relation)
        {
            if (expressions.Length <= 1) throw new Exception("too few arguments.");
            var current = expressions[0].Evaluate(scope) as SNumber;
            foreach(var obj in expressions.Skip(1))
            {
                var next = obj.Evaluate(scope) as SNumber;
                if (relation(current, next))
                    current = next;
                else
                    return SBool.False;
            }
            return SBool.True;
        }
    }


    public class SScope
    {
        public SScope Parent { get; private set; }
        private Dictionary<string, SObject> _symbolTable;

        public static Dictionary<string, Func<SExpression[], SScope, SObject>> BuiltinFunctions
        {
            get; private set;
        } = new Dictionary<string, Func<SExpression[], SScope, SObject>>();

        static SScope()
        {
            //initilize builtin functions
            BuildIn("+", (args, scope) => (args.Evaluate<SNumber>(scope).Sum(s => s)));
            BuildIn("-", (args, scope) =>
             {
                 var numbers = args.Evaluate<SNumber>(scope).ToArray();
                 var firstValue = numbers[0];
                 if (numbers.Length == 1) return -firstValue;
                 return firstValue - numbers.Skip(1).Sum(s => s);
             });
            BuildIn("first", (args, scope) =>
             {
                 SList list = null;
                 if (args.Length != 1 || (list = (args[0].Evaluate(scope) as SList)) == null) throw new Exception("param must be a SList.");
                 return list.First();
             });
            BuildIn("=", (args, scope) => args.ChainRelation(scope, (s1, s2) => (Int64)s1 == (Int64)s2));
        }
              
        public SScope(SScope parent)
        {
            Parent = parent;
            _symbolTable = new Dictionary<string, SObject>();
        }

        public SObject Find(string name)
        {
            var current = this;
            while(current != null)
            {
                if (current._symbolTable.ContainsKey(name))
                    return current._symbolTable[name];
                current = current.Parent;
            }
            throw new Exception(name + " is not found.");
        }

        public static void BuildIn(string name, Func<SExpression[],SScope,SObject> builtinFunction)
        {
            BuiltinFunctions.Add(name, builtinFunction);          
        }

        public SObject Define(string name,SObject value)
        {
            _symbolTable.Add(name, value);
            return value;
        }

        public SObject Reset(string name,SObject value)
        {
            _symbolTable[name] = value;
            return value;
        }

        public SScope SpawnScopeWith(string[] names, SObject[] values)
        {
            if (names.Length < values.Length) throw new Exception("too many arguments");
            var scope = new SScope(this);
            foreach (var i in Enumerable.Range(1, values.Length))
                scope._symbolTable.Add(names[i], values[i]);
            return scope;
        }

        public SObject FindInTop(string name)
        {
            if (_symbolTable.ContainsKey(name))
                return _symbolTable[name];
            return null;
        }
    }
}
