using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public interface IExpressionAlgorithm
    {
        void Visit(CharSetExpression expression);
        void Visit(LoopExpression expression);
        void Visit(SequenceExpression expression);
        void Visit(AlternativeExpression expression);
        void Visit(BeginExpression expression);
        void Visit(EndExpression expression);
        void Visit(CaptureExpression expression);
        void Visit(MatchExpression expression);
        void Visit(PositiveExpression expression);
        void Visit(NegativeExpression expression);
        void Visit(ReferenceExpression expression);
    }

    public abstract class RegexExpressionAlgorithm<ReturnT, ParamT> : IExpressionAlgorithm
    {
        private ReturnT _returnValue;
        private ParamT _paramValue;

        public ReturnT Invoke(Expression expression, ParamT param)
        {
            _paramValue = param;
            expression.Apply(this);
            return _returnValue;
        }

        public abstract ReturnT Apply(CharSetExpression expression, ParamT param);
        public abstract ReturnT Apply(LoopExpression expression, ParamT param);
        public abstract ReturnT Apply(SequenceExpression expression, ParamT param);
        public abstract ReturnT Apply(AlternativeExpression expression, ParamT param);
        public abstract ReturnT Apply(BeginExpression expression, ParamT param);
        public abstract ReturnT Apply(EndExpression expression, ParamT param);
        public abstract ReturnT Apply(CaptureExpression expression, ParamT param);
        public abstract ReturnT Apply(MatchExpression expression, ParamT param);
        public abstract ReturnT Apply(PositiveExpression expression, ParamT param);
        public abstract ReturnT Apply(NegativeExpression expression, ParamT param);
        public abstract ReturnT Apply(ReferenceExpression expression, ParamT param);

        public void Visit(CharSetExpression expression)
        {          
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(LoopExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(SequenceExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(AlternativeExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(BeginExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(EndExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(CaptureExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(MatchExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(PositiveExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(NegativeExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }

        public void Visit(ReferenceExpression expression)
        {
            _returnValue = this.Apply(expression, _paramValue);
        }
    }
}
