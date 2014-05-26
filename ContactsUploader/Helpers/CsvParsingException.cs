using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContactsUploader.Helpers
{
    /// <summary>
    /// Exception during CSV file parsing
    /// </summary>
    public class CsvParsingException : Exception
    {
        /// <summary>
        /// File offset position where the parsing error had occurred
        /// </summary>
        public int FilePosition { get; set; }

        public CsvParsingException(string message)
            : base(message)
        { }

        public CsvParsingException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}