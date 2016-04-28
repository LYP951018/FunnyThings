using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchemeInterpreter
{
    public class SObject
    {
        public static implicit operator SObject(Int64 value)
        {
            return (SNumber)value;
        }
    }

    public class SNumber : SObject
    {
        private readonly Int64 _value;

        public SNumber(Int64 value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Int64(SNumber number)
        {
            return number._value;
        }

        public static implicit operator SNumber(Int64 value)
        {
            return new SNumber(value);
        }
    }

    public class SBool : SObject
    {
        public static readonly SBool False = new SBool();
        public static readonly SBool True = new SBool();

        public override string ToString()
        {
            return ((bool)this).ToString();
        }

        public static implicit operator bool(SBool value)
        {
            return value == SBool.True;
        }

        public static implicit operator SBool(Boolean value)
        {
            return value ? True : False;
        }

    }

    public class SFunction : SObject
    {
        public SExpression Body { get; private set; }
        public string[] Parameters { get; private set; }
        public SScope Scope { get; private set; }

        public bool IsPartial
        {
            get
            {
                return ComputeFilledParameters().Length != Parameters.Length;
            }
        }

        public SFunction Update(SObject[] arguments)
        {
            var exsitingArguments = Parameters.Select(param => Scope.FindInTop(param)).Where(obj => obj != null);
            var newArguments = exsitingArguments.Concat(arguments).ToArray();
            var newScope = Scope.Parent.SpawnScopeWith(Parameters, newArguments);
            return new SFunction(Body, Parameters, newScope);
        }

        public SFunction(SExpression body, string[] parameters, SScope scope)
        {
            Body = body;
            Parameters = parameters;
            Scope = scope;
        }

        public SObject Evaluate()
        {
            var filledParams = ComputeFilledParameters();
            if (filledParams.Length < Parameters.Length)
                return this;
            else
                return Body.Evaluate(Scope);
        }

        public string[] ComputeFilledParameters()
        {
            return Parameters.Where(p => Scope.FindInTop(p) != null).ToArray();
        }
    }


    public class SList : SObject, IEnumerable<SObject>
    {
        private readonly IEnumerable<SObject> _values;

        public SList(IEnumerable<SObject> values)
        {
            _values = values;
        }

        public override string ToString()
        {
            return $"(list {string.Join(" ", _values)})";
        }

        public IEnumerator<SObject> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }
}
