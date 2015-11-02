using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public class Transition
    {
        public enum Type
        {
            Chars,
            Epsilon,
            BeginString, //^
            EndString, //$
            Nop, //控制优先级
            Capture,
            Match,
            Positive,
            Negative,
            NegativeFail,
            End
        }

        public State Start { get; set; }
        public State End { get; set; }
        public CharRange Range { get; set; }
        public Type TransitionType { get; set; }
        public int Capture { get; set; } = 0;
        public int Index { get; set; } = 0;

        public Transition(State start, State end, CharRange range, Type transitionType, int capture, int index)
        {
            Start = start;
            End = end;
            Range = range;
            TransitionType = transitionType;
            Capture = capture;
            Index = index;
        }

        public Transition()
            : this(null, null)
        {

        }

        public Transition(State start, State end)
            : this(start, end, Type.Epsilon)
        {

        }

        public Transition(State start, State end, Type transitionType)
            : this(start, end, default(CharRange), transitionType, 0, 0)
        {

        }

        public Transition(State start, State end, CharRange range)
            : this(start, end, range, Type.Chars, 0, 0)
        {

        }

        public override string ToString()
        {
            switch(TransitionType)
            {
                case Type.BeginString:
                    return "Begin";
                case Type.Capture:
                    return "Capture" + Capture;
                case Type.Chars:
                    {
                        var sb = new StringBuilder();
                        sb.Append('[').Append(Range.Begin)
                            .Append(" to ").Append(Range.End == char.MaxValue ? "+∞" : Range.End.ToString()).Append(']');                        
                        return sb.ToString();
                    }
                case Type.End:
                    return "End";
                case Type.EndString:
                    return "EndString";
                case Type.Epsilon:
                    return "ε";
                case Type.Match:
                    return "Index: " + Index;
                case Type.Negative:
                    return "Negative";
                case Type.NegativeFail:
                    return "NegativeFail";
                case Type.Nop:
                    return "Nop";
                case Type.Positive:
                    return "Positive";           
            }
            return string.Empty;
        }

        //static Automaton RemoveEpsilon(Automaton source)
    }

    public class State
    {
        public List<Transition> Input { get; set; }
        public List<Transition> Output { get; set; }
        public bool IsFinalState { get; set; }
        public object UserData { get; set; }

        public State()
        {
            Input = new List<Transition>();
            Output = new List<Transition>();
            IsFinalState = false;
            UserData = null;
        }
    }

    public class Automaton
    {
        public List<State> States { get; } = new List<State>();
        public List<Transition> Transitions { get; } = new List<Transition>();
        public List<string> CaptureNames { get; } = new List<string>();
        public State StartState { get; set; }

        public State AddState()
        {
            var ret = new State();
            States.Add(ret);
            return ret;
        }

        private void AddInternal(State start, State end, Transition transition)
        {
            start.Output.Add(transition);
            end.Input.Add(transition);
            Transitions.Add(transition);
        }

        public Transition AddTransition(State start, State end, Transition.Type transitionType)
        {
            var transition = new Transition(start, end, transitionType);
            AddInternal(start, end, transition);
            return transition;
        }

        public Transition AddMatch(State start, State end, int capture, int index)
        {
            var transition = new Transition(start, end, default(CharRange), Transition.Type.Match, capture, index);
            AddInternal(start, end, transition);
            return transition;
        }

        public Transition AddCapture(State start, State end, int capture)
        {
            var transition = new Transition(start, end, default(CharRange), Transition.Type.Capture, capture, 0);
            AddInternal(start, end, transition);
            return transition;
        }

        public Transition AddCharRange(State start, State end, CharRange range)
        {
            var transition = new Transition(start, end, range);
            AddInternal(start, end, transition);
            return transition;
        }
    }

}
