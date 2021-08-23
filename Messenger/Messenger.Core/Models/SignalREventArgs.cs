using System;

namespace Messenger.Core.Models
{
    /// <summary>
    /// Represents event arguments to use with signalR functions
    /// </summary>
    public class SignalREventArgs<T> : EventArgs
    {
        /// <summary>
        /// The value of an signalR event argument
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Build a signalR event argument with one value
        /// </summary>
        public SignalREventArgs(T value)
        {
            Value = value;
        }
    }

    public class SignalREventArgs<TOne, TTwo> : EventArgs
    {
        /// <summary>
        /// The first value of an signalR event argument
        /// </summary>
        public TOne FirstValue { get; set; }

        /// <summary>
        /// The second value of an signalR event argument
        /// </summary>
        public TTwo SecondValue { get; set; }

        /// <summary>
        /// Build a signalR event argument with two values
        /// </summary>
        public SignalREventArgs(TOne firstValue, TTwo secondValue)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
        }
    }
}
