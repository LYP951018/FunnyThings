using System;

namespace Protocol
{
    public class PayloadEventArgs<T> : EventArgs
    {
        private T _body;

        public PayloadEventArgs(T body)
        {
            _body = body;
        }

        public T Body => _body;
    }
}
