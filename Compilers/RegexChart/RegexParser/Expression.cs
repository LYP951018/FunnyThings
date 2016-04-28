using System.Collections.Generic;
using System.Text;

namespace RegexChart.RegexParser
{
    public abstract class Expression
    {
        public abstract void Apply(IExpressionAlgorithm algorithm);

        public void NormalizeCharSet(out FlatSet<CharRange> subsets)
        {
            var normalized = new NormalizedCharSet();
            new BuildNormalizedCharSetAlgorithm().Invoke(this, normalized);
            new SetNormalizedCharSetAlgorithm().Invoke(this, normalized);
            subsets = new FlatSet<CharRange>(normalized.Ranges);
        }

        public Automaton GenerateExpsilonNfa()
        {
            var automation = new Automaton();
            var result = new EpsilonNfaAlgorithm().Invoke(this, automation);
            automation.StartState = result.Start;
            result.End.IsFinalState = true;
            return automation;
        }

    }

    //字符集合
    //\d == [0-9] etc.
    public class CharSetExpression : Expression
    {
        public FlatSet<CharRange> Ranges { get; set; }
        public bool IsReverse { get; set; }        

        public bool AddRangeWithConflict(char begin,char end)
        {
            var range = new CharRange(begin, end);
            //交叉
            foreach (var r in Ranges)
            {
                if (!(range < r || range > r)) return false;
            }
            Ranges.Add(range);
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            foreach(var r in Ranges)
            {
                sb.Append(r.Begin);
                sb.Append('-');
                sb.Append(r.End).Append(',');
            }
            sb.Append(']');
            return sb.ToString();
        }

        public CharSetExpression()
        {
            Ranges = new FlatSet<CharRange>();
            IsReverse = false;
        }

        public CharSetExpression(CharSetExpression expression)
        {
            Ranges = new FlatSet<CharRange>(expression.Ranges);
            IsReverse = expression.IsReverse;
        }

        public void Add(char begin,char end)
        {
            Ranges.Add(new CharRange(begin, end));
        }

        public void Add(char c)
        {
            Ranges.Add(new CharRange(c));
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((CharSetExpression)obj);
        }

        public bool Equals(CharSetExpression obj)
        {
            if (IsReverse != obj.IsReverse) return false;
            if (Ranges.Count != obj.Ranges.Count) return false;
            for(int i = 0;i < Ranges.Count;++i)
            {
                if (Ranges[i] != obj.Ranges[i])
                    return false;
            }
            return true;
        }
    }

    //重复
    public class LoopExpression : Expression
    {
        public Expression Looped { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public bool IsGreedy { get; set; }

        public LoopExpression(int min,int max,bool isGreedy,Expression looped = null)
        {
            Min = min;
            Max = max;
            IsGreedy = isGreedy;
            Looped = looped;
        }

        public LoopExpression(LoopExpression expression)
        {
            Looped = expression.Looped;
            Min = expression.Min;
            Max = expression.Max;
            IsGreedy = expression.IsGreedy;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((LoopExpression)obj);
        }

        public bool Equals(LoopExpression obj)
        {
            if (Min != obj.Min) return false;
            if (Max != obj.Max) return false;
            if (IsGreedy != obj.IsGreedy) return false;
            return Looped.Equals(obj.Looped);
        }
    }

    //串联
    public class SequenceExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public SequenceExpression(Expression left,Expression right)
        {
            Left = left;
            Right = right;
        }

        public SequenceExpression()
        {
            Left = null;
            Right = null;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((SequenceExpression)obj);
        }

        public bool Equals(SequenceExpression obj)
        {
            return
                Left.Equals(obj.Left) &&
                Right.Equals(obj.Right);
        }
    }

    public class AlternativeExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public AlternativeExpression(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((AlternativeExpression)obj);
        }

        public bool Equals(AlternativeExpression obj)
        {
            return Left.Equals(obj.Left) &&
                Right.Equals(obj.Right);
        }
    }

    public class BeginExpression : Expression
    {
        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((BeginExpression)obj);
        }

        public bool Equals(BeginExpression obj)
        {
            return true;
        }
    }

    public class EndExpression : Expression
    {
        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((EndExpression)obj);
        }

        public bool Equals(EndExpression obj)
        {
            return true;
        }
    }

    public class CaptureExpression : Expression
    {
        public string Name { get; set; }
        public Expression Sub { get; set; }

        public CaptureExpression(string name, Expression sub)
        {
            Name = name;
            Sub = sub;
        }

        //匿名
        public CaptureExpression(Expression sub)
        {
            Name = null;       
            Sub = sub;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((CaptureExpression)obj);
        }

        public bool Equals(CaptureExpression obj)
        {
            return
                Name == obj.Name &&
                Sub.Equals(obj.Sub);
        }
    }

    public class PositiveExpression : Expression
    {
        public Expression Matched { get; set; }

        public PositiveExpression(Expression subExpression)
        {
            Matched = subExpression;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((PositiveExpression)obj);
        }

        public bool Equals(PositiveExpression obj)
        {
            return Matched.Equals(obj.Matched);
        }
    }

    public class NegativeExpression : Expression
    {
        public Expression Matched { get; set; }

        public NegativeExpression(Expression subExpression)
        {
            Matched = subExpression;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return this.Equals((NegativeExpression)obj);
        }

        public bool Equals(NegativeExpression obj)
        {
            return Matched.Equals(obj.Matched);
        }
    }

    public class ReferenceExpression : Expression
    {
        public string Name { get; set; }

        public ReferenceExpression(string name)
        {
            Name = name;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }
    }

    public class RegexExpression
    {
        public Dictionary<string, Expression> Definitions { get; set; }
        public Expression Main { get; set; }

        public Expression Merge()
        {
            var param = new MergeParameter();
            param.Main = this;
            return new MergeAlgoritm().Invoke(Main, param);
        }
    }

    public class MatchExpression : Expression
    {

        public string Name { get; set; }
        public int Index { get; set; }

        public MatchExpression(string name,int index)
        {
            Name = name;
            Index = index;
        }

        public MatchExpression(int index)
        {
            Name = null;
            Index = index;
        }

        public MatchExpression(MatchExpression expression)
        {
            Name = expression.Name;
            Index = expression.Index;
        }

        public override void Apply(IExpressionAlgorithm algorithm)
        {
            algorithm.Visit(this);
        }
    }

}
