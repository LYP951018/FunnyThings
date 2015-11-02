using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public class RegexNode
    {
        public Expression Exp { get; set; }

        public RegexNode(Expression expression)
        {
            Exp = expression;
        }

        public RegexNode Loop(int min,int max)
        {
            var loop = new LoopExpression(min, max, true, Exp);
            return new RegexNode(loop);
        }

        public RegexNode Some()
        {
            return Loop(1, -1);
        }

        public RegexNode Any()
        {
            return Loop(0, -1);
        }
       
        public RegexNode Opt()
        {
            return Loop(0, 1);
        }    

        public RegexNode AtLeast(int min)
        {
            return Loop(min, -1);
        }

        public static RegexNode operator + (RegexNode node1,RegexNode node2)
        {
            var expression = new SequenceExpression(node1.Exp, node2.Exp);
            return new RegexNode(expression);
        }

        public static RegexNode operator | (RegexNode node1,RegexNode node2)
        {
            var expression = new AlternativeExpression(node1.Exp, node2.Exp);
            return new RegexNode(expression);
        }

        public static RegexNode operator + (RegexNode node)
        {
            return new RegexNode(new PositiveExpression(node.Exp));
        }

        public static RegexNode operator -(RegexNode node)
        {
            return new RegexNode(new NegativeExpression(node.Exp));
        }

        public static RegexNode operator !(RegexNode node)
        {
            var exp = node.Exp as CharSetExpression;
            if(exp != null)
            {
                var expression = new CharSetExpression();
                foreach (var r in exp.Ranges)
                {
                    expression.Ranges.Add(r);
                }
                expression.IsReverse = !exp.IsReverse;
                return new RegexNode(expression);
            }
            return null;
        }

        public static RegexNode operator %(RegexNode node1, RegexNode node2)
        {
            var left = node1.Exp as CharSetExpression;
            var right = node2.Exp as CharSetExpression;

            Debug.Assert(left != null && right != null && !left.IsReverse && !right.IsReverse);

            var expression = new CharSetExpression();
            expression.IsReverse = false;
            foreach (var r in left.Ranges)
            {
                expression.Ranges.Add(r);
            }

            foreach(var r in right.Ranges)
            {
                if(!expression.AddRangeWithConflict(r.Begin,r.End))
                {
                    Debug.Assert(false, "Failed");
                }
            }

            return new RegexNode(expression);
        }

        public static RegexNode GetCapture(string name,RegexNode node)
        {
            var expression = new CaptureExpression(name, node.Exp);
            return new RegexNode(expression);
        }

        public static RegexNode GetReference(string name)
        {
            var expression = new ReferenceExpression(name);
            return new RegexNode(expression);
        }

        public static RegexNode GetMatch(string name,int index)
        {
            var expression = new MatchExpression(name, index);
            return new RegexNode(expression);
        }

        public static RegexNode GetMatch(int index)
        {
            var expression = new MatchExpression(index);
            return new RegexNode(expression);
        }

        public static RegexNode GetBegin()
        {
            return new RegexNode(new BeginExpression());
        }

        public static RegexNode GetEnd()
        {
            return new RegexNode(new EndExpression());
        }

        public static RegexNode GetCharSetExpression(char a,char b)
        {
            var expression = new CharSetExpression();
            expression.AddRangeWithConflict(a, b);
            return new RegexNode(expression);
        }

        public static RegexNode GetCharSetExpression(char a)
        {
            return GetCharSetExpression(a, a);
        }

        public static RegexNode GetDigitSet()
        {
            return GetCharSetExpression('0', '9');
        }

        public static RegexNode GetAlphaSet()
        {
            return GetCharSetExpression('a','z') % GetCharSetExpression('A','Z') % GetCharSetExpression('_');
        }
        public static RegexNode GetDigitAndAlphaSet()
        {
            return GetCharSetExpression('0', '9') % GetCharSetExpression('a', 'z') % GetCharSetExpression('A', 'Z') % GetCharSetExpression('_');
        }

        public static RegexNode GetAnyChar()
        {
            return GetCharSetExpression((char)(1), char.MaxValue);
        }
    }
}
