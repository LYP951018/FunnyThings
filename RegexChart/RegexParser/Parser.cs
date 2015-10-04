using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    class Parser
    {
        private SlidingTextWindow _sourceWindow;
        public LoopExpression ParseLoop()
        {
            int min = 0, max = 0;
            var c = _sourceWindow.PeekChar();
            if (c == SlidingTextWindow.InvalidCharacter)
                return null;
            if (_sourceWindow.AdvanceIfMatches('+'))
            {
                min = 1;
                max = -1;
            }
            else if (_sourceWindow.AdvanceIfMatches('*'))
            {
                min = 0;
                max = -1;
            }
            else if (_sourceWindow.AdvanceIfMatches('?'))
            {
                min = 0;
                max = 1;
            }
            else if (_sourceWindow.AdvanceIfMatches('{'))
            {
                int result = 0;
                if (_sourceWindow.AdvanceIfPositiveInteger(out result))
                {
                    if (_sourceWindow.AdvanceIfMatches(','))
                    {
                        if (!_sourceWindow.AdvanceIfPositiveInteger(out max))
                        {
                            max = -1;
                        }
                    }
                    else
                    {
                        max = min;
                    }
                    if (!_sourceWindow.AdvanceIfMatches('}'))
                        throw new ArgumentException("} lost.");
                }
                else throw new ArgumentException("invalid {}");
            }
            else return null;
            return new LoopExpression(min, max, _sourceWindow.AdvanceIfMatches('?'));

        }

        public Expression ParseCharSet()
        {        
            if (_sourceWindow.PeekChar() == SlidingTextWindow.InvalidCharacter)
                return null;
            if (_sourceWindow.AdvanceIfMatches('^'))
                return new BeginExpression();
            else if (_sourceWindow.AdvanceIfMatches('$'))
                return new EndExpression();
            else if (_sourceWindow.AdvanceIfMatches('.'))
            {
                var ret = new CharSetExpression();
                ret.Add(char.MinValue, (char)(char.MaxValue - 1));
                return ret;
            }
            else if (_sourceWindow.AdvanceIfMatches('\\') || _sourceWindow.AdvanceIfMatches('/'))
            {
                //\d etc.
                var exp = new CharSetExpression();
                var c2 = _sourceWindow.PeekChar();
                switch (c2)
                {
                    case 'r':
                        exp.Add('\r');
                        break;
                    case 'n':
                        exp.Add('\n');
                        break;
                    case 't':
                        exp.Add('\t');
                        break;
                    //需要转义的字符在这里
                    case '\\':
                    case '/':
                    case '(':
                    case ')':
                    case '+':
                    case '*':
                    case '?':
                    case '|':
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case '<':
                    case '>':
                    case '^':
                    case '$':
                    case '!':
                    case '=':
                    case '.':
                        exp.Add(c2);
                        break;
                    case 'S':
                        exp.IsReverse = true;
                        goto case 's';
                    case 's':
                        //spaces
                        exp.Add(' ');
                        exp.Add('\r');
                        exp.Add('\n');
                        exp.Add('\t');
                        break;
                    case 'D':
                        exp.IsReverse = true;
                        goto case 'd';
                    case 'd':
                        exp.Add('0', '9');
                        break;
                    case 'L':
                        exp.IsReverse = true;
                        goto case 'l';
                    case 'l':
                        exp.Add('_');
                        exp.Add('A', 'Z');
                        exp.Add('a', 'z');
                        break;
                    case 'W':
                        exp.IsReverse = true;
                        goto case 'w';
                    case 'w':
                        exp.Add('0', '9');
                        exp.Add('_');
                        exp.Add('A', 'Z');
                        exp.Add('a', 'z');
                        break;
                    default:
                        throw new ArgumentException("Error character after \\");
                }
                _sourceWindow.AdvanceChar();
                return exp;
            }
            //stuff like [a-z]
            else if(_sourceWindow.AdvanceIfMatches('['))
            {
                var exp = new CharSetExpression();
                exp.IsReverse = _sourceWindow.AdvanceIfMatches('^');
                bool midState = false;
                char lhs = default(char), rhs = default(char);
                while(true)
                {
                    if (_sourceWindow.AdvanceIfMatches('\\') || _sourceWindow.AdvanceIfMatches('/'))
                    {
                        var c = _sourceWindow.PeekChar();
                        char tmp = default(char);
                        switch (c)
                        {
                            case 'r':
                                tmp = '\r';
                                break;
                            case 'n':
                                tmp = '\n';
                                break;
                            case 't':
                                tmp = '\t';
                                break;
                            //需要转义的字符在这里
                            case '\\':
                            case '/':
                            case '(':
                            case ')':
                            case '+':
                            case '*':
                            case '?':
                            case '|':
                            case '{':
                            case '}':
                            case '[':
                            case ']':
                            case '<':
                            case '>':
                            case '^':
                            case '$':
                            case '!':
                            case '=':
                            case '.':
                                tmp = c;
                                break;
                            default:
                                throw new ArgumentException("Error syntax in []");
                        }
                        _sourceWindow.AdvanceChar();
                        if (midState)
                            rhs = c;
                        else
                            lhs = c;
                        midState = !midState;
                    }
                    else if (_sourceWindow.AdvanceIfMatches("-]"))
                        throw new ArgumentException("-] occurred.");
                    else 
                    {
                        var c2 = _sourceWindow.NextChar();
                        if(_sourceWindow.IsValid(c2))
                        {
                            if (midState)
                                rhs = c2;
                            else
                                lhs = c2;
                            midState = !midState;
                        }
                        else throw new ArgumentException("Error in []");
                    }
                    if (_sourceWindow.AdvanceIfMatches(']'))
                    {
                        if (midState)
                            rhs = lhs;
                        if (!exp.AddRangeWithConflict(lhs, rhs))
                            throw new ArgumentException();
                        break;
                    }
                    else if(_sourceWindow.AdvanceIfMatches('-'))
                    {
                        if (!midState)
                            throw new ArgumentException("Invalid - in []");
                    }   
                    else
                    {
                        var c2 = _sourceWindow.NextChar();
                        if(_sourceWindow.IsValid(c2))
                        {
                            if (midState)
                            {
                                rhs = lhs;
                            }
                            if (exp.AddRangeWithConflict(lhs, rhs))
                                    midState = false;
                            else throw new ArgumentException();
                            
                        }
                    }
                }
            }
                
        }
    }
}
