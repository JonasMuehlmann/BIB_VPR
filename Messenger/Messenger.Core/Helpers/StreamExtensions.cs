using System;
using System.IO;

namespace Messenger.Core.Helpers
{
    /// <summary>
    /// Holds static methods for efficient data streaming
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Convert a stream to a base64 encoded string
        /// </summary>
        /// <param name="stream">The stream to encode</param>
        /// <returns>The base64 representation of stream</returns>
        public static string ToBase64String(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
