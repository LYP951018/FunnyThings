using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    public class SExpression
    {
        public string Value { get; private set; }
        public List<SExpression> Children { get; private set; }
        public SExpression Parent { get; private set; }

        public SExpression(string value, SExpression parent)
        {
            Value = value;
            Children = new List<SExpression>();
            Parent = parent;
        }

        public override string ToString()
        {
            if (Value == "(")
                return $"({string.Join(" ", Children)})";
            else return Value;
        }

        public SObject Evaluate(SScope scope)
        {
            if(Children.Count == 0)
            {
                Int64 number;
                if (Int64.TryParse(Value, out number))
                    return number;
                else
                    return scope.Find(Value);
            }
            else
            {
                var first = Children[0].Value;
                if (first == "list")
                {
                    return new SList(this.Children.Skip(1).Select(exp => exp.Evaluate(scope)));
                }
                else if(first == "if")
                {
                    var condition = Children[1].Evaluate(scope) as SBool;
                    return condition ? Children[2].Evaluate(new SScope(scope)) :Children[3].Evaluate(new SScope(scope));
                }
                else if(first == "define")
                {
                    var child = Children[1];
                    if (child.Value == "(")
                    {
                        var funcName = child.Children[0].Value;
                        var body = Children[2];
                        var parameters = child.Children.Skip(1).Select(exp => exp.Value).ToArray();                      
                        var newScope = new SScope(scope);
                        var func = new SFunction(body, parameters, newScope);
                        return scope.Define(funcName,func);
                    }
                    return scope.Define(Children[1].Value, Children[2].Evaluate(scope));
                }
                else if (first == "set!")
                {
                    var variableName = Children[1].Value;
                    var newValue = Children[2];
                    if (scope.Find(variableName) != null)
                    {
                        return scope.Reset(variableName, newValue.Evaluate(scope));
                    }
                }     
                else if(first == "begin")
                {
                    SObject result = null;
                    foreach (var statement in Children.Skip(1))
                        result = statement.Evaluate(scope);
                    return result;
                }           
                else if (SScope.BuiltinFunctions.ContainsKey(first))
                {
                    var arguments = Children.Skip(1).ToArray();
                    return SScope.BuiltinFunctions[first](arguments, scope);
                }
                else
                {
                    var func = first == "(" ? /*High-order function*/Children[0].Evaluate(scope) as SFunction
                        : scope.Find(first) as SFunction;
                    var arguments = Children.Skip(1).Select(s => s.Evaluate(scope)).ToArray();
                    return func.Update(arguments).Evaluate();
                }
            }
            
            throw new Exception("Sorry!");
        }
    }
}
