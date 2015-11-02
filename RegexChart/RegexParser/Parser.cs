using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    public class Parser
    {
        private SlidingTextWindow _sourceWindow;

        public Parser(string source)
        {
            _sourceWindow = new SlidingTextWindow(source);
        }


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
                    min = result;
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
            //greedy or not
            return new LoopExpression(min, max, !_sourceWindow.AdvanceIfMatches('?'));

        }

        //be responsible for ^ $ . \b(etc.) [a-z] and any character
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
                ret.Add((char)1,char.MaxValue);
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
            else if (_sourceWindow.AdvanceIfMatches('['))
            {
                var exp = new CharSetExpression();
                exp.IsReverse = _sourceWindow.AdvanceIfMatches('^');
                bool midState = false;
                char lhs = default(char), rhs = default(char);
                while (true)
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
                        if (_sourceWindow.IsValid(c2))
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
                    else if (_sourceWindow.AdvanceIfMatches('-'))
                    {
                        if (!midState)
                            throw new ArgumentException("Invalid - in []");
                    }
                    else
                    {
                        var c2 = _sourceWindow.NextChar();
                        if (_sourceWindow.IsValid(c2))
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
                return exp;
            }
            else
            {
                char c = default(char);
                if (_sourceWindow.AdvanceIfOneOf("()+*?{}|",out c))
                {
                    _sourceWindow.AdvanceChar(-1);
                    return null;
                }
                else
                {
                    //the character itself
                    var exp = new CharSetExpression();
                    exp.IsReverse = false;
                    exp.Add(_sourceWindow.NextChar());
                    return exp;
                }    
            }

            
        }

        public Expression ParseUnit()
        {
            //first,find the loop part
            var unit = ParseCharSet();
            if(unit == null)
            {
                unit = ParseFunction();
            }
            if (unit == null) return null;
            //then,loop
            LoopExpression loop;
            while(true)
            {
                loop = ParseLoop();
                if (loop == null) break;
                loop.Looped = unit;
                unit = loop;
            }
            return unit;
        }

        public Expression ParseJoin()
        {
            var left = ParseUnit();
            while(true)
            {
                var right = ParseUnit();
                if (right != null)
                {
                    var seqExpression = new SequenceExpression(left,right);                   
                    left = seqExpression;
                }
                else break;
            }
            return left;
        }

        public Expression ParseAlternate()
        {
            var left = ParseJoin();
            while(true)
            {
                if (_sourceWindow.AdvanceIfMatches('|'))
                {
                    var right = ParseJoin();
                    if (right == null)
                    {
                        throw new ArgumentException("Expect expression after |.");
                    }
                    var seq = new AlternativeExpression(left, right);
                    left = seq;
                }
                else break;
            }
            return left;
        }

        public Expression ParseExpression()
        {
            return ParseAlternate();
        }

        Expression ParseFunction()
        {
            if (_sourceWindow.AdvanceIfMatches("(="))
            {
                var sub = ParseExpression();
                if (!_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost");
                }
                var exp = new PositiveExpression(sub);
                return exp;
            }
            else if (_sourceWindow.AdvanceIfMatches("(!"))
            {
                var sub = ParseExpression();
                if (!_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost");
                }
                var exp = new NegativeExpression(sub);
                return exp;
            }
            else if (_sourceWindow.AdvanceIfMatches("(<&"))
            {
                //表达式引用
                string name;
                if (!_sourceWindow.AdvanceIfName(out name))
                {
                    throw new ArgumentException("invalid name.");
                }
                if (!_sourceWindow.AdvanceIfMatches('>'))
                {
                    throw new ArgumentException("> lost.");
                }
                if (!_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost.");
                }
                var exp = new ReferenceExpression(name);
                return exp;
            }
            else if (_sourceWindow.AdvanceIfMatches("(<$"))
            {
                string name;
                int index = -1;
                if (_sourceWindow.AdvanceIfName(out name))
                {
                    if (_sourceWindow.AdvanceIfMatches(';'))
                    {
                        if (!_sourceWindow.AdvanceIfPositiveInteger(out index))
                        {
                            throw new ArgumentException("Positive numbers required after ;");
                        }
                    }
                }
                else if (!_sourceWindow.AdvanceIfPositiveInteger(out index))
                {
                    throw new ArgumentException("Positive numbers required after (<$");
                }
                if (!_sourceWindow.AdvanceIfMatches('>'))
                {
                    throw new ArgumentException("> lost.");
                }
                if (!_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost.");
                }
                var exp = new MatchExpression(name, index);
                return exp;
            }
            else if (_sourceWindow.AdvanceIfMatches("(<"))
            {
                string name;
                if (!_sourceWindow.AdvanceIfName(out name))
                {
                    throw new ArgumentException("Name lost");
                }
                if (!_sourceWindow.AdvanceIfMatches('>'))
                {
                    throw new ArgumentException("> lost");
                }
                var sub = ParseExpression();
                if (!_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost");
                }
                var exp = new CaptureExpression(name, sub);
                return exp;
            }
            else if (_sourceWindow.AdvanceIfMatches("(?"))
            {
                var sub = ParseExpression();
                if (_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost");
                }
                var exp = new CaptureExpression(sub);
                return exp;
            }
            else if (_sourceWindow.AdvanceIfMatches('('))//subexpression
            {
                var exp = ParseExpression();
                if (!_sourceWindow.AdvanceIfMatches(')'))
                {
                    throw new ArgumentException(") lost");
                }
                return exp;
            }
            else return null;
        }

        RegexExpression ParseRegexExpression(string source)
        {
            var regex = new RegexExpression();
            try
            {
                while(_sourceWindow.AdvanceIfMatches("(<#"))
                {
                    string name;
                    if(!_sourceWindow.AdvanceIfName(out name))
                    {
                        throw new ArgumentException("Identifier lost.");
                    }
                    if(!_sourceWindow.AdvanceIfMatches('>'))
                    {
                        throw new ArgumentException("> lost.");
                    }
                    var sub = ParseExpression();
                    if(!_sourceWindow.AdvanceIfMatches(')'))
                    {
                        throw new ArgumentException(") needed");
                    }
                    if(regex.Definitions.Keys.Contains(name))
                    {
                        throw new ArgumentException("name duplicated.");
                    }
                    else
                    {
                        regex.Definitions.Add(name, sub);
                    }
                }
                regex.Main = ParseExpression();
                if(regex.Main == null)
                {
                    throw new ArgumentException("Expression Expected.");
                }
                if(_sourceWindow.HasMoreChars())
                {
                    throw new ArgumentException("Syntax error.");
                }
                return regex;
            }
            catch(ArgumentException e)
            {
                //...
                throw new ArgumentException(e.Message);
            }
        }

        //make \r-> \\r
        string EscapeTextForRegex(string literalString)
        {
            var sb = new StringBuilder(literalString.Length);
            foreach(var c in literalString)
            {
                switch(c)
                {
                    case '\\':case '/':case '(':case ')':case '+':case '*':case '?':case '|':
				    case '{':case '}':case '[':case ']':case '<':case '>':
				    case '^':case '$':case '!':case '=':
                        sb.Append('\\').Append(c);
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        //\\r->\r
        string UnescapeTextForRegex(string escapedText)
        {
            var sb = new StringBuilder(escapedText.Length);
            int i = 0;
            char c;
            while(true)
            {
                if (i >= escapedText.Length) break;
                c = escapedText[i];
                if (c == '\\' || c == '/')
                {
                    if (i + 1 < escapedText.Length)
                    {
                        ++i;
                        c = escapedText[i];
                        switch (c)
                        {
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            default:
                                sb.Append(c);
                                break;
                        }
                        continue;
                    }
                }
                sb.Append(c);
                ++i;
            }
            return sb.ToString();
        }
    }
}
