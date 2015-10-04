using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexChart.RegexParser
{
    class SlidingTextWindow
    {
        public const char InvalidCharacter = char.MaxValue;

        private int _offset;
        private string _sourceText;

        public void AdvanceChar()
        {
            _offset++;
        }

        public void AdvanceChar(int n)
        {
            _offset += n;
        }

        public char PeekChar()
        {
            if(_offset >= _sourceText.Length)
                return InvalidCharacter;          
            return _sourceText[_offset];
        }

        public void Reset(int position)
        {
            Debug.Assert(position < _sourceText.Length, "Invalid position.");
            _offset = position;
        }

        public char PeekChar(int delta)
        {
            var tmp = _offset;
            this.AdvanceChar(delta);
            char ret;
            if (_offset >= _sourceText.Length)
                ret = InvalidCharacter;
            else
                ret = _sourceText[_offset];
            this.Reset(tmp);
            return ret;
        }

        public char NextChar()
        {
            var c = PeekChar();
            if (c != InvalidCharacter)
                this.AdvanceChar();
            return c;
        }

        public bool AdvanceIfMatches(char c)
        {
            if (NextChar() == c)
            {
                AdvanceChar();
                return true;
            }
            return false;
        }

        public bool AdvanceIfMatches(string desired)
        {
            var length = desired.Length;
            for (int i = 0; i < length; i++)
            {
                if (PeekChar(i) != desired[i])
                    return false;
            }
            AdvanceChar(length);
            return true;
        }

        public bool AdvanceIfPositiveInteger(out int result)
        {
            result = 0;
            int i = 0;
            char c;
            while(true)
            {
                c = PeekChar(i);
                if (char.IsDigit(c))
                    result = 10 * result + (int)char.GetNumericValue(c);               
                else break;
                i++;
            }
            AdvanceChar(i);
            return i != 0;   
        }

        public bool AdvanceIfName(out string result)
        {
            var sb = new StringBuilder();
            result = null;
            int i = 1;
            char c = PeekChar();
            //以下划线或字母开头
            if (char.IsLetter(c) || c == '_')
            {
                sb.Append(c);
                while (true)
                {
                    c = PeekChar(i);
                    if (char.IsLetterOrDigit(c) || c == '_')
                        sb.Append(c);
                    else break;
                    i++;
                }
            }
            else return false;
            result = sb.ToString();
            AdvanceChar(i);
            return true;
        }

        public bool IsValid(char c)
        {
            return c != InvalidCharacter;
        }
    }


}
