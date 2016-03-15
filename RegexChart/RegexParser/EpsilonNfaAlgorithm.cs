using System;

namespace RegexChart.RegexParser
{

    class EpsilonNfaInfo
    {
        public Automaton NfaAutomation { get; } = new Automaton();
    }

    class EpsilonNfa
    {
        public State Start { get; set; }
        public State End { get; set; }

        public EpsilonNfa()
        {
            Start = null;
            End = null;
        }

        public EpsilonNfa(EpsilonNfa rhs)
        {
            Start = rhs.Start;
            End = rhs.End;
        }

        public EpsilonNfa(State start, State end)
        {
            Start = start;
            End = end;
        }

        public EpsilonNfa(Automaton automaton)
        {
            Start = automaton.AddState();
            End = automaton.AddState();
        }

    }


    class EpsilonNfaAlgorithm : RegexExpressionAlgorithm<EpsilonNfa, Automaton>
    {
        public EpsilonNfa Connect(EpsilonNfa a, EpsilonNfa b, Automaton target)
        {
            if (a.Start != null)
            {
                var ret = new EpsilonNfa(a);
                target.AddTransition(a.End, b.Start, Transition.Type.Epsilon);
                ret.End = b.End;
                return ret;
            }
            return new EpsilonNfa(b);
        }

        public override EpsilonNfa Apply(SequenceExpression expression, Automaton param)
        {
            return Connect(
                Invoke(expression.Left, param),
                Invoke(expression.Right, param), param);
        }

        public override EpsilonNfa Apply(BeginExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa(param);
            param.AddTransition(nfa.Start, nfa.End, Transition.Type.BeginString);
            return nfa;
        }

        public override EpsilonNfa Apply(CaptureExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa(param);
            int captureIndex = 0;
            if (expression.Name.Length != 0)
            {
                var names = param.CaptureNames;
                captureIndex = names.IndexOf(expression.Name);
                if (captureIndex == -1)
                {
                    captureIndex = names.Count;
                    names.Add(expression.Name);
                }
            }
            var body = Invoke(expression.Sub, param);
            param.AddCapture(nfa.Start, body.Start, captureIndex);
            param.AddTransition(body.End, nfa.End, Transition.Type.End);
            return nfa;
        }

        public override EpsilonNfa Apply(PositiveExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa(param);
            var body = Invoke(expression.Matched, param);
            param.AddTransition(nfa.Start, body.Start, Transition.Type.Positive);
            param.AddTransition(body.End, nfa.End, Transition.Type.End);
            return nfa;
        }

        public override EpsilonNfa Apply(ReferenceExpression expression, Automaton param)
        {
            throw new NotImplementedException();
        }

        public override EpsilonNfa Apply(NegativeExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa(param);
            var body = Invoke(expression.Matched, param);
            param.AddTransition(nfa.Start, body.Start, Transition.Type.Negative);
            param.AddTransition(body.End, nfa.End, Transition.Type.Negative);
            param.AddTransition(nfa.Start, nfa.End, Transition.Type.NegativeFail);
            return nfa;
        }

        public override EpsilonNfa Apply(MatchExpression expression, Automaton param)
        {
            int captureIndex = -1;
            if (expression.Name.Length != 0)
            {
                var names = param.CaptureNames;
                captureIndex = names.IndexOf(expression.Name);
                if (captureIndex == -1)
                {
                    captureIndex = names.Count;
                    names.Add(expression.Name);
                }
            }

            var res = new EpsilonNfa(param);
            param.AddMatch(res.Start, res.End, captureIndex, expression.Index);
            return res;
        }

        public override EpsilonNfa Apply(EndExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa(param);
            param.AddTransition(nfa.Start, nfa.End, Transition.Type.EndString);
            return nfa;
        }

        public override EpsilonNfa Apply(AlternativeExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa(param);
            var a = Invoke(expression.Left, param);
            var b = Invoke(expression.Right, param);
            param.AddTransition(nfa.Start, a.Start, Transition.Type.Epsilon);
            param.AddTransition(nfa.Start, b.Start, Transition.Type.Epsilon);
            param.AddTransition(a.End, nfa.End, Transition.Type.Epsilon);
            param.AddTransition(b.End, nfa.End, Transition.Type.Epsilon);
            return nfa;
        }

        public override EpsilonNfa Apply(LoopExpression expression, Automaton param)
        {
            var nfa = new EpsilonNfa();
            for (int i = 0; i < expression.Min; ++i)
            {
                var body = Invoke(expression.Looped, param);
                nfa = Connect(nfa, body, param);
            }
            if (expression.Max == -1)
            {
                var body = Invoke(expression.Looped, param);
                if (nfa.Start == null)
                {
                    nfa.Start = nfa.End = param.AddState();
                }
                var loopBegin = nfa.End;
                var loopEnd = param.AddState();
                if (expression.IsGreedy)
                {
                    param.AddTransition(loopBegin, body.Start, Transition.Type.Epsilon);
                    param.AddTransition(body.End, loopBegin, Transition.Type.Epsilon);
                    param.AddTransition(loopBegin, loopEnd, Transition.Type.Nop);
                }
                else
                {
                    param.AddTransition(loopBegin, loopEnd, Transition.Type.Nop);
                    param.AddTransition(loopBegin, body.Start, Transition.Type.Epsilon);
                    param.AddTransition(body.End, loopBegin, Transition.Type.Epsilon);
                }
                nfa.End = loopEnd;
            }
            else if (expression.Min < expression.Max)
            {
                for (int i = expression.Min; i < expression.Max; ++i)
                {
                    var body = Invoke(expression.Looped, param);
                    var start = param.AddState();
                    var end = param.AddState();
                    if (expression.IsGreedy)
                    {
                        param.AddTransition(start, body.Start, Transition.Type.Epsilon);
                        param.AddTransition(body.End, end, Transition.Type.Epsilon);
                        param.AddTransition(start, end, Transition.Type.Nop);
                    }
                    else
                    {
                        param.AddTransition(start, end, Transition.Type.Nop);
                        param.AddTransition(start, body.Start, Transition.Type.Epsilon);
                        param.AddTransition(body.End, end, Transition.Type.Epsilon);
                    }
                    body.Start = start;
                    body.End = end;
                    nfa = Connect(nfa, body, param);
                }
            }
            return nfa;
        }

        public override EpsilonNfa Apply(CharSetExpression expression, Automaton param)
        {
            //[a-z A-Z] 是或，两个 State 之间有许多 range 边
            var nfa = new EpsilonNfa(param);
            foreach (var r in expression.Ranges)
            {
                param.AddCharRange(nfa.Start, nfa.End, r);
            }
            return nfa;

        }
    }
}
