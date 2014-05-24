using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContactsUploader.Helpers
{
    public class CsvParsingException : Exception
    {
        public int FilePosition { get; set; }

        public CsvParsingException(string message)
            : base(message)
        { }

        public CsvParsingException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}