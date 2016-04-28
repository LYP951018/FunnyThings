using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    class StateInfo
    {
        public enum SaveReason
        {
            Other,
            Positive,
            Negative
        }

        public StateInfo(StateInfo other)
        {
            Position = other.Position;
            Current = other.Current;
            MinAvaliableTransition = other.MinAvaliableTransition;
            Reason = other.Reason;
        }

        public StateInfo()
        { }

        public int Position { get; set; }
        public State Current { get; set; }
        public int MinAvaliableTransition { get; set; }
        public SaveReason Reason { get; set; }
    }

    class ExtensionStateInfo : StateInfo
    {
       public int CaptureListIndex { get; set; }
       public Transition SavedTransition { get; set; }
    }

    class CaptureRecord
    {
        public int CaptureIndex { get; set; }
        public int StartIndex { get; set;}
        public int MatchLength { get; set; }
    }

    struct UserData
    {
        public bool ShouldBeSaved { get; set; }
    }

    class RichResult
    {
        public int Start { get; set; }
        public int length { get; set; }
        public List<CaptureRecord> Records { get; set; }
    }

    class RichInterpreter
    {
        private Automaton _dfa;
        private UserData[] _userData = new UserData[2]
        {
            new UserData { ShouldBeSaved = false },
            new UserData { ShouldBeSaved = true },
        };

        public RichInterpreter(Automaton dfa)
        {
            _dfa = dfa;

            foreach(var state in dfa.States)
            {
                int charEdgeCount = 0;
                bool hasPositive = false;
                foreach(var transition in state.Output)
                {
                    //几种情况：
                    //1. 对于只有一种非字符边的，那么没必要回溯了：说明根本不匹配。
                    //2. 对于有若干种非字符边的，应该保存状态，以备回溯。
                    //3. Positive 保存下来，匹配成功了要回到这里来。
                    if (transition.TransitionType == Transition.Type.Chars)
                        charEdgeCount++;
                    else if(transition.TransitionType == Transition.Type.Positive)
                    {
                        hasPositive = true;
                        break;
                    }
                }
                state.UserData = _userData[(hasPositive || charEdgeCount > 1 || (charEdgeCount != 0 && state.Output.Count != charEdgeCount)) ? 1 : 0];
            }
        }

        private bool Match(string input, int startIndex, out RichResult result)
        {
            result = new RichResult();
            var normalStateRecords = new List<StateInfo>();
            var extStateRecords = new List<ExtensionStateInfo>();
            var currentState = new StateInfo
            {
                Position = 0,
                Current = _dfa.StartState,
                MinAvaliableTransition = 0,
                Reason = StateInfo.SaveReason.Other
            };

            normalStateRecords.Add(currentState);
            
            while (!currentState.Current.IsFinalState)
            {
                bool found = false;
                var stateToSave = new StateInfo(currentState);
                for(int i = 0; i < currentState.Current.Output.Count; ++i) 
                {
                    var transition = currentState.Current.Output[i];
                    switch (transition.TransitionType)
                    {
                        case Transition.Type.Chars:
                            var c = input[currentState.Position];
                            var range = transition.Range;
                            if (c >= range.Begin && c <= range.End)
                            {
                                currentState.Current = transition.End;
                                currentState.Position++;
                            }
                            else found = false;
                            break;
                        case Transition.Type.BeginString:
                            found = currentState.Position == 0;
                            break;
                        case Transition.Type.EndString:
                            found = currentState.Position == input.Length - 1;
                            break;
                        case Transition.Type.Nop:
                            found = true;
                            break;
                        case Transition.Type.Capture:
                            {
                                var captureRecord = new CaptureRecord
                                {
                                    CaptureIndex = transition.Capture,
                                    StartIndex = currentState.Position,
                                    MatchLength = -1
                                };
                                result.Records.Add(captureRecord);
                                var extStateInfo = new ExtensionStateInfo
                                {
                                    Position = currentState.Position,
                                    SavedTransition = transition
                                };
                                extStateRecords.Add(extStateInfo);
                                found = true;
                            }                            
                            break;
                        case Transition.Type.Match:
                            int index = 0;
                            foreach(var record in result.Records)
                            {
                                if(record.CaptureIndex == transition.Capture)
                                {
                                    if(record.MatchLength != -1 && (transition.Index == -1 || transition.Index == index))
                                    {
                                        if (input.Substring(record.StartIndex, record.MatchLength) == input.Substring(currentState.Position, record.MatchLength))
                                        {
                                            currentState.Position += record.MatchLength;
                                            found = true;
                                            break;
                                        }                                       
                                    }
                                    else ++index;
                                }
                            }
                            break;
                        case Transition.Type.Positive:
                            {
                                var extStateInfo = new ExtensionStateInfo
                                {
                                    Position = currentState.Position,
                                    SavedTransition = transition
                                };

                                extStateRecords.Add(extStateInfo);
                                stateToSave.Reason = StateInfo.SaveReason.Positive;
                            }
                            break;
                        case Transition.Type.End:
                            var prevState = extStateRecords.Last();
                            extStateRecords.RemoveAt(extStateRecords.Count - 1);
                            switch(prevState.SavedTransition.TransitionType)
                            {
                                case Transition.Type.Capture:
                                    {
                                        var capture = result.Records[prevState.CaptureListIndex];
                                        capture.MatchLength = currentState.Position - prevState.Position;
                                        found = true;
                                    }
                                    break;
                                case Transition.Type.Positive:
                                    {
                                        //Positive 结束后，回到初始 Positive 状态。
                                        for(int j = normalStateRecords.Count - 1; j >=0; --j)
                                        {
                                            var state = normalStateRecords[j];
                                            if(state.Reason == StateInfo.SaveReason.Positive)
                                            {
                                                currentState.Position = state.Position;
                                                stateToSave.Position = state.Position;
                                                break;
                                            }
                                        }
                                        found = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    if (found)
                    {
                        //有一条边是可以过的。
                        var userData = (UserData)currentState.Current.UserData;
                        if (userData.ShouldBeSaved == true)
                        {
                            stateToSave.MinAvaliableTransition = i + 1;
                            normalStateRecords.Add(stateToSave);
                        }
                        currentState.Current = transition.End;
                        currentState.MinAvaliableTransition = 0;
                    }
                }
                //试过了所有的边，都没有找到，回溯。
                if (!found)
                {
                    if (normalStateRecords.Count != 0)
                    {
                        currentState = normalStateRecords.Last();
                        normalStateRecords.RemoveAt(normalStateRecords.Count - 1);
                    }
                    else
                        break;
                }              
            }

            if(currentState.Current.IsFinalState)
            {
                result.Start = startIndex;
                result.length = currentState.Position - startIndex;
                result.Records.Remove()
            }
        }
    }
}
