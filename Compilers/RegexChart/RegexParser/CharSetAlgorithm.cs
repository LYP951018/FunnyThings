namespace RegexChart.RegexParser
{
    public class NormalizedCharSet
    {
        public FlatSet<CharRange> Ranges { get; set; }

        public NormalizedCharSet()
        {
            Ranges = new FlatSet<CharRange>();
        }

        public void AddRange(char begin,char end)
        {
            Ranges.Add(new CharRange(begin, end));
        }
    }

    public abstract class CharSetAlgorithm : RegexExpressionAlgorithm<bool, NormalizedCharSet>
    {
        public abstract void Process(CharSetExpression expression , NormalizedCharSet target, CharRange range);

        //ranges == expression.Ranges
        public void Loop(CharSetExpression expression, FlatSet<CharRange> ranges, NormalizedCharSet target)
        {
            if(expression.IsReverse)
            {
                char begin = (char)(char.MinValue + 1);
                foreach(var r in ranges)
                {
                    if(r.Begin > begin)
                    {
                        Process(expression, target, new CharRange(begin, (char)(r.Begin - 1)));
                    }
                    begin = (char)(r.End + 1);
                }
                if (begin <= char.MaxValue)
                    Process(expression, target, new CharRange(begin, char.MaxValue));
            }
            else
            {
                foreach (var r in ranges)
                    Process(expression, target, r);
            }
        }

        public override bool Apply(SequenceExpression expression, NormalizedCharSet param)
        {
            Invoke(expression.Left, param);
            Invoke(expression.Right, param);
            return false;
        }

        public override bool Apply(BeginExpression expression, NormalizedCharSet param)
        {
            return false;
        }

        public override bool Apply(CaptureExpression expression, NormalizedCharSet param)
        {
            Invoke(expression.Sub,param);
            return false;
        }

        public override bool Apply(PositiveExpression expression, NormalizedCharSet param)
        {
            Invoke(expression.Matched, param);
            return false;
        }

        public override bool Apply(ReferenceExpression expression, NormalizedCharSet param)
        {
            return false;
        }

        public override bool Apply(NegativeExpression expression, NormalizedCharSet param)
        {
            return false;
        }

        public override bool Apply(MatchExpression expression, NormalizedCharSet param)
        {
            return false;
        }

        public override bool Apply(EndExpression expression, NormalizedCharSet param)
        {
            return false;
        }

        public override bool Apply(AlternativeExpression expression, NormalizedCharSet param)
        {
            Invoke(expression.Left, param);
            Invoke(expression.Right, param);
            return false;
        }

        public override bool Apply(LoopExpression expression, NormalizedCharSet param)
        {
            Invoke(expression.Looped,param);
            return false;
        }

        //public abstract bool Apply(CharSetExpression expression, NormalizedCharSet param);
    }

    public class BuildNormalizedCharSetAlgorithm : CharSetAlgorithm
    {
        public override bool Apply(CharSetExpression expression, NormalizedCharSet target)
        {
            Loop(expression, expression.Ranges, target);
            return false;
        }

        public override void Process(CharSetExpression expression, NormalizedCharSet target, CharRange range)
        {
            int index = 0;
            while(index < target.Ranges.Count)
            {
                var cur = target.Ranges[index];
                var ranges = target.Ranges;
                if (!cur.IsOverlapped(range))
                {
                    index++;
                    continue;
                }

                else if (cur.Begin < range.Begin)
                {
                    //cur:  |---------|
                    //range:   |---------?
                    //切割成两半
                    //插入两半后，index 自增后演变成：
                    //cur:  |--|-------|
                    //range:   |---------?
                    ranges.RemoveAt(index);
                    target.AddRange(cur.Begin, (char)(range.Begin - 1));
                    target.AddRange(range.Begin, cur.End);
                    index++;
                }
                else if (cur.Begin > range.Begin)
                {
                    //cur:        |----|
                    //range:   |---------?
                    target.AddRange(range.Begin, (char)(cur.Begin - 1));
                    range.Begin = cur.Begin;
                    index++;
                }
                else if (cur.End < range.End)
                {
                    //cur:        |----|
                    //range:      |---------|
                    range.Begin = (char)(cur.End + 1);
                    index++;
                }
                else if (cur.End > range.End)
                {
                    //cur:        |-----------|
                    //range:      |---------|
                    target.Ranges.RemoveAt(index);
                    target.Ranges.Add(range);
                    target.AddRange((char)(range.End + 1), cur.End);
                    return;
                }
                else return;
            }
            target.Ranges.Add(range);
        }
    }

    public class SetNormalizedCharSetAlgorithm : CharSetAlgorithm
    {
        public override bool Apply(CharSetExpression expression, NormalizedCharSet param)
        {
            var source = new FlatSet<CharRange>(expression.Ranges);          
            expression.Ranges.Clear();
            Loop(expression, source, param);
            expression.IsReverse = false;
            return false;
        }

        public override void Process(CharSetExpression expression, NormalizedCharSet target, CharRange range)
        {
            foreach(var r in target.Ranges)
            {
                if(range.Begin <= r.Begin && r.End <= range.End)
                {
                    expression.Ranges.Add(r);
                }
            }
        }
    }
}
