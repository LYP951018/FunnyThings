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
        
        public Transition(Transition transition)
        {
            Start = transition.Start;
            End = transition.End;
            Range = transition.Range;
            TransitionType = transition.TransitionType;
            Capture = transition.Capture;
            Index = transition.Index;
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

        public void AddState(State state)
        {
            States.Add(state);
        }

        private void AddInternal(State start, State end, Transition transition)
        {
            start.Output.Add(transition);
            end.Input.Add(transition);
            Transitions.Add(transition);
        }

        public void AddTransition(Transition transition)
        {
            AddInternal(transition.Start, transition.End, transition);
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

        public static Automaton RemoveEpsilon(Automaton source)
        {
            //标记所有输入的边都没有消耗字符的状态
            //TODO: copy captured names
            List<Transition> transitions = new List<Transition>();
            var oldToNew = new Dictionary<State, State>();
            var newToOld = new Dictionary<State, State>();
            var nfa = new Automaton();
            var epsilonStates = new List<State>();
            var newStartstate = nfa.AddState();
            oldToNew.Add(source.StartState, newStartstate);
            newToOld.Add(newStartstate, source.StartState);
            nfa.StartState = newStartstate;
            
            for(int i = 0;i < nfa.States.Count;++i)
            {
                var state = nfa.States[i];
                var oldState = newToOld[state];
                FindPath(oldState, state, transitions, epsilonStates);
                foreach(var trans in transitions)
                {
                    var target = trans.End;
                    if(!oldToNew.Keys.Contains(target))
                    {
                        var newState = nfa.AddState();
                        if (state.IsFinalState) newState.IsFinalState = true;
                        oldToNew.Add(target, newState);
                        newToOld.Add(newState, target);
                    }
                    var newTransition = new Transition(trans);
                    newTransition.Start = state;
                    newTransition.End = oldToNew[target];
                    nfa.AddTransition(newTransition);
                }
                transitions.Clear();
                epsilonStates.Clear();
            }
            return nfa;
        }

        public static bool IsConsume(Transition transition)
        {
            var type = transition.TransitionType;
            return type != Transition.Type.Epsilon ;
        }

        public static void FindPath(State source,State target,List<Transition> transitions,List<State> epsilonStates)
        {
            if(!epsilonStates.Contains(source))
            {
                epsilonStates.Add(source);
                foreach(var t in source.Output)
                {
                    if(!IsConsume(t))
                    {
                        if(!epsilonStates.Contains(t.End))
                        {
                            if (t.End.IsFinalState) target.IsFinalState = true;
                            FindPath(t.End, target, transitions, epsilonStates);
                        }
                    }
                    else
                    {
                        transitions.Add(t);
                    }
                }
            }
        }

        public static void Add<T> (Dictionary<T,List<T>> dictionary,T key,T value)
        {
            if(dictionary.Keys.Contains(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                var newList = new List<T>();
                dictionary.Add(key, newList);
                newList.Add(value);
            }
        }

        public static Automaton NfaToDfa(Automaton nfa)
        {
            return null;
        }
    }

}
