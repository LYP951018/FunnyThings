using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RegexChart.RegexParser
{
    public class Transition
    {
        public enum Type
        {
            Chars,
            Epsilon,
            BeginString,            //^
            EndString,              //$
            Nop,                    //控制优先级
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
            //Start = transition.Start;
            //End = transition.End;
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

        static public bool operator ==(Transition lhs,Transition rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) return false;
            if (lhs.TransitionType != rhs.TransitionType) return false;
            switch(lhs.TransitionType)
            {
                case Type.Chars:
                    return lhs.Range == rhs.Range;
                case Type.Capture:
                    return lhs.Capture == rhs.Capture;
                case Type.Match:
                    return lhs.Capture == rhs.Capture && lhs.Index == rhs.Index;
                default:
                    return true;
            }
        }

        public override int GetHashCode()
        {
            switch (TransitionType)
            {
                case Type.Chars:
                    return Range.GetHashCode();
                case Type.Capture:
                    return Capture.GetHashCode();
                case Type.Match:
                    return (Capture.GetHashCode() + Index.GetHashCode()).GetHashCode();
                default:
                    return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            return this == (Transition)obj;
        }

        static public bool operator !=(Transition lhs, Transition rhs)
        {
            return !(lhs == rhs);
        }
    }

    [DebuggerDisplay("Index = {Index}")]
    public class State
    {
#if DEBUG
        private static int _globalIndex;
        int Index { get; set; }
#endif
        public List<Transition> Input { get; set; }
        public List<Transition> Output { get; set; }
        public bool IsFinalState { get; set; }
        public object UserData { get; set; }

        public State()
        {
#if DEBUG
            Index = _globalIndex;
            ++_globalIndex;
#endif
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
        public List<string> CaptureNames { get; set; } = new List<string>();
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
            var nfa = new Automaton();
            nfa.CaptureNames = source.CaptureNames;
            var newStartstate = nfa.AddState();
            nfa.StartState = newStartstate;
            List<Transition> transitions = new List<Transition>();
            var oldToNew = new Dictionary<State, State>
            {
                [source.StartState] = newStartstate
            };
            var newToOld = new Dictionary<State, State>()
            {
                [newStartstate] = source.StartState
            };                     
            var epsilonStates = new List<State>();      
                            
            for(int i = 0; i < nfa.States.Count; ++i)
            {
                var state = nfa.States[i];
                var oldState = newToOld[state];
                FindPath(oldState, state, transitions, epsilonStates);
                if (oldState.IsFinalState) state.IsFinalState = true;
                foreach (var trans in transitions)
                {
                    var target = trans.End;
                    if(!oldToNew.Keys.Contains(target))
                    {
                        var newState = nfa.AddState();                      
                        oldToNew.Add(target, newState);
                        newToOld.Add(newState, target);
                    }
                    var newTransition = nfa.AddTransition(state, oldToNew[target], trans.TransitionType);
                    newTransition.Range = trans.Range;
                    newTransition.Capture = trans.Capture;
                    newTransition.Index = trans.Index;
                }
                transitions.Clear();
                epsilonStates.Clear();
            }
            return nfa;
        }

        public static bool IsConsume(Transition transition)
        {
            return transition.TransitionType != Transition.Type.Epsilon;
        }

        public static void FindPath(State source, State target, List<Transition> transitions, List<State> epsilonStates)
        {
            if(!epsilonStates.Contains(source))
            {
                epsilonStates.Add(source);
                foreach(var t in source.Output)
                {
                    if(!IsConsume(t))
                    {
                        if (t.End.IsFinalState) target.IsFinalState = true;
                        FindPath(t.End, target, transitions, epsilonStates);
                    }
                    else
                        transitions.Add(t);
                }
            }
        }
               
        public static Automaton NfaToDfa(Automaton nfa, out MultiValueDictionary<State, State> dfaStatesToNfa)
        {
            var dfa = new Automaton();
            dfa.CaptureNames = nfa.CaptureNames;
            var dfaStartState = dfa.AddState();
            dfa.StartState = dfaStartState;
            var nfaTransitionsToDfa = new MultiValueDictionary<Transition, Transition>();
            var transitionClasses = new List<Transition>();
            var mergeStates = new HashSet<State>();
            dfaStatesToNfa = new MultiValueDictionary<State, State>();
            dfaStatesToNfa.Add(dfaStartState, nfa.StartState);
            //不动点算法，不能使用 foreach！
            for(int i = 0; i < dfa.States.Count; ++i)
            {
                var curDfaState = dfa.States[i];
                var nfaStates = dfaStatesToNfa[curDfaState];
                foreach(var nfaState in nfaStates)
                {
                    foreach(var outTransition in nfaState.Output)
                    {                         
                        if (!nfaTransitionsToDfa.Values.SelectMany(transitions => transitions).Contains(outTransition))
                        {
                            transitionClasses.Add(outTransition);
                            nfaTransitionsToDfa.Add(outTransition, outTransition);
                        }
                    }
                }

                foreach(var transitionClass in transitionClasses)
                {
                    var nfaTransitions = nfaTransitionsToDfa[transitionClass];
                    foreach(var nfaTransition in nfaTransitions)
                    {
                        var state = nfaTransition.End;
                        if (!mergeStates.Contains(state))
                            mergeStates.Add(state);
                    }

                    mergeStates.OrderBy(state => state);

                    //mergeStates 是候选的一个 DFA 状态。在这之前，还需要判断：这个候选状态是否已经存在于 DFA 中了？
                    //var isContained = dfaStatesToNfa.Values.Contains((IReadOnlyCollection<State>)mergeStates);
                    State newDfaState = null;
                    foreach(var dfaState in dfaStatesToNfa.Keys)
                    {
                        var prevNfaStates = dfaStatesToNfa[dfaState];
                        if (prevNfaStates.Count == mergeStates.Count &&
                            prevNfaStates.SequenceEqual(mergeStates))
                        {
                            newDfaState = dfaState;
                            break;
                        }
                    }
                    if(newDfaState == null)
                    {
                        newDfaState = dfa.AddState();
                        dfaStatesToNfa.AddRange(newDfaState, mergeStates);
                        newDfaState.IsFinalState = mergeStates.Any(state => state.IsFinalState);
                    }

                    var dfaTransition = dfa.AddTransition(curDfaState, newDfaState, transitionClass.TransitionType);
                    dfaTransition.Capture = transitionClass.Capture;
                    dfaTransition.Range = transitionClass.Range;
                    dfaTransition.Index = transitionClass.Index;
                    mergeStates.Clear();
                }
                transitionClasses.Clear();                
                nfaTransitionsToDfa.Clear();
            }

            return dfa;
        }
    }

}
