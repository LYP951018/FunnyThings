using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public static class GroupOperations
    {
        public static void GroupAdd<T>(this Dictionary<T, List<T>> dictionary, T key, T value)
        {
            if (dictionary.Keys.Contains(key))
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
                    newTransition.TransitionType = trans.TransitionType;
                }
                transitions.Clear();
                epsilonStates.Clear();
            }
            return nfa;
        }

        public static bool IsConsume(Transition transition)
        {
            var type = transition.TransitionType;
            return type != Transition.Type.Epsilon;
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

        
        public static Automaton NfaToDfa(Automaton nfa, out Dictionary<State, List<State>> statesMap)
        {
            //NFA 转 DFA 时，一条边对应多个边（合并同样功能的边）
            var dfaToNfaTransitionsMap = new Dictionary<Transition, List<Transition>>();

            //TODO: capture names!
            var target = new Automaton();
            var dfaStartState = target.AddState();
            var transitionClasses = new List<Transition>();
            statesMap = new Dictionary<State, List<State>>();
            var targetStates = new List<State>();

            statesMap.GroupAdd(dfaStartState, nfa.StartState);
            target.StartState = dfaStartState;

            for(int i = 0;i < target.States.Count;++i)
            {
                var curState = target.States[i];
                dfaToNfaTransitionsMap.Clear();
                transitionClasses.Clear();

                var nfaStates = statesMap[curState];

                foreach(var nfaState in nfaStates)
                {
                    foreach (var transition in nfaState.Output)
                    {
                        Transition transitionClass = null;
                        foreach (var keyTransition in dfaToNfaTransitionsMap.Keys)
                        {
                            if (keyTransition == transition)
                            {
                                transitionClass = keyTransition;
                                break;
                            }
                        }
                        if (transitionClass == null)
                        {
                            dfaToNfaTransitionsMap.GroupAdd(transition, transition);
                            transitionClasses.Add(transition);
                        }
                    }

                }

                foreach (var transitionClass in transitionClasses)
                {
                    var transitionsList = dfaToNfaTransitionsMap[transitionClass];
                    //合并相同的 transition 所指的状态。
                    targetStates.Clear();
                    foreach(var transition in  transitionsList)
                    {
                        var state = transition.End;
                        if (!targetStates.Contains(state))
                            targetStates.Add(state);
                    }
                    State finalDfaState = null;
                    //判断是否已经存在于当前 targetStates 一模一样的 DFA 状态                  
                    foreach(var kv in statesMap)
                    {
                        var states = kv.Value;
                        if(states.Count != targetStates.Count 
                            || states.GetHashCode() != targetStates.GetHashCode()
                            || states.Intersect(targetStates).Count() != states.Count)
                        {
                            finalDfaState = kv.Key;
                            break;
                        }
                    }

                    if(finalDfaState == null)
                    {
                        finalDfaState = target.AddState();
                        foreach(var state in targetStates)
                        {
                            if (state.IsFinalState)
                                finalDfaState.IsFinalState = true;
                            statesMap.GroupAdd(finalDfaState, state);
                        }
                    }
                    target.AddTransition(transitionClass);
                }
            }

            return target;
        }
    }

}
