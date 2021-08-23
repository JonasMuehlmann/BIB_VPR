using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Messenger.Core.Helpers
{
    /// <summary>
    /// Holds static methods to work with JSON
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Deserialize a JSON encoded string to an instance of a type
        /// </summary>
        /// <param name="value">The JSON encoded string to deserialize</param>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <returns>The object resulting from the deserialization</returns>
        public static async Task<T> ToObjectAsync<T>(string value)
        {
            return await Task.Run<T>(() =>
            {
                return JsonConvert.DeserializeObject<T>(value);
            });
        }

        /// <summary>
        /// Serialize an object to a JSON encoded string
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <returns>The JSON encoded string resulting from the serialization</returns>
        public static async Task<string> StringifyAsync(object value)
        {
            return await Task.Run<string>(() =>
            {
                return JsonConvert.SerializeObject(value);
            });
        }
    }
}
