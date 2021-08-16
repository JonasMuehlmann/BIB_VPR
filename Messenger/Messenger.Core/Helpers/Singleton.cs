using System;
using System.Collections.Concurrent;

namespace Messenger.Core.Helpers
{
    /// <summary>
    /// Represents a singleton instance holding an object
    /// </summary>
    /// <typeparam name="T">The type to store in the instance</typeparam>
    public static class Singleton<T>
        where T : new()
    {
        private static ConcurrentDictionary<Type, T> _instances = new ConcurrentDictionary<Type, T>();

        /// <summary>
        /// Access the singleton instance's type
        /// </summary>
        public static T Instance
        {
            get
            {
                return _instances.GetOrAdd(typeof(T), (t) => new T());
            }
        }
    }
}
