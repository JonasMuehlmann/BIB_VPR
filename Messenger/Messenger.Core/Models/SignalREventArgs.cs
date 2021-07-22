using System;

namespace Messenger.Core.Models
{
    public class SignalREventArgs<T> : EventArgs
    {
        public T Value { get; set; }

        public SignalREventArgs(T value)
        {
            Value = value;
        }
    }

    public class SignalREventArgs<TOne, TTwo> : EventArgs
    {
        public TOne FirstValue { get; set; }

        public TTwo SecondValue { get; set; }

        public SignalREventArgs(TOne firstValue, TTwo secondValue)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
        }
    }
}
