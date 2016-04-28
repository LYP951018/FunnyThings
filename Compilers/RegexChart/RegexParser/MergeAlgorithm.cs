using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public class MergeParameter
    {
        public Dictionary<string, Expression> Definitions { get; set; }
        public RegexExpression Main { get; set; }
    }

    public class MergeAlgoritm : RegexExpressionAlgorithm<Expression, MergeParameter>
    {
        //copy the entire syntax tree
        public override Expression Apply(SequenceExpression expression, MergeParameter param)
        {
            var result = new SequenceExpression(
                Invoke(expression.Left, param),
                Invoke(expression.Right, param));
            return result;
        }

        public override Expression Apply(BeginExpression expression, MergeParameter param)
        {
            return new BeginExpression();
        }

        public override Expression Apply(CaptureExpression expression, MergeParameter param)
        {
            return new CaptureExpression(
                expression.Name,
                Invoke(expression.Sub, param));
        }

        public override Expression Apply(PositiveExpression expression, MergeParameter param)
        {
            return new PositiveExpression(
                Invoke(expression, param));
        }

        public override Expression Apply(ReferenceExpression expression, MergeParameter keysHasCollected)
        {
            if(keysHasCollected.Definitions.ContainsKey(expression.Name))
            {
                var ret = keysHasCollected.Definitions[expression.Name];
                if (ret != null) return ret;
                else throw new ArgumentException("loop reference");
            }
            else if(keysHasCollected.Main.Definitions.ContainsKey(expression.Name))
            {
                keysHasCollected.Definitions.Add(expression.Name, null);
                var ret = Invoke(keysHasCollected.Main.Definitions[expression.Name], keysHasCollected);
                keysHasCollected.Definitions[expression.Name] = ret;
                return ret;
            }
            else
                throw new ArgumentException($"Cannot find the referenced expression {expression.Name}.");
        }

        public override Expression Apply(NegativeExpression expression, MergeParameter param)
        {
            return new NegativeExpression(Invoke(expression.Matched, param));
        }

        public override Expression Apply(MatchExpression expression, MergeParameter param)
        {
            return new MatchExpression(expression);
        }

        public override Expression Apply(EndExpression expression, MergeParameter param)
        {
            return new EndExpression();
        }

        public override Expression Apply(AlternativeExpression expression, MergeParameter param)
        {
            return new AlternativeExpression(
                Invoke(expression.Left, param),
                Invoke(expression.Right, param));
        }

        public override Expression Apply(LoopExpression expression, MergeParameter param)
        {
            return new LoopExpression(
                expression.Min,
                expression.Max,
                expression.IsGreedy,
                Invoke(expression.Looped, param));
        }

        public override Expression Apply(CharSetExpression expression, MergeParameter param)
        {
            return new CharSetExpression(expression);
        }
    }
}
