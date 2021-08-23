using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Messenger.Core.Models
{
    /// <summary>
    /// Holds a Stream of data and a file path intended to safe the stream's data to
    /// </summary>
    public class UploadData
    {
        /// <summary>
        /// Holds a stream of data intended as a files content
        /// </summary>
        public Stream StreamFile { get; set; }
        /// <summary>
        /// Holds A file path intended to safe the stream's data to
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Fully initialize an UploadData instance
        /// </summary>
        public UploadData(Stream streamFile, string filePath)
        {
            StreamFile = streamFile;
            FilePath = filePath;
        }

        public override string ToString()
        {
            return $"UploadData: StreamFile={StreamFile}, FilePath={FilePath}";
        }
    }
}
