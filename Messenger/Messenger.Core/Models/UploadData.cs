using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Messenger.Core.Models
{
    public class UploadData
    {
        public Stream StreamFile { get; set; }
        public string FilePath { get; set; }

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
