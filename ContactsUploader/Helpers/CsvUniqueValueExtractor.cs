using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ContactsUploader.Helpers
{
    /// <summary>
    /// Given a csv file and a column (field) index, extracts the set of unique string values from that column
    /// </summary>
    public class CsvUniqueValueExtractor
    {
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string _filename;
        private bool _hasHeaders;
        
        /// <summary>
        /// Number of invalid values omitted from the last result returned
        /// </summary>
        public int NumDiscardedValues { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename">filename of the csv file to parse</param>
        /// <param name="hasHeaders">whether the csv file given has a first row header</param>
        public CsvUniqueValueExtractor(string filename, bool hasHeaders)
        {
            _filename = filename;
            _hasHeaders = hasHeaders;
        }

        /// <summary>
        /// Extracts a set of unique values from the csv file at the given column index
        /// </summary>
        /// <param name="fieldIndex"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUnique(int fieldIndex)
        {
            return GetUnique(fieldIndex, (s) => true);
        }

        /// <summary>
        /// Extracts a set of unique values from the csv file at the given column index. Omits values that are invalidated by the supplied delegate.
        /// </summary>
        /// <param name="fieldIndex"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUnique(int fieldIndex, Func<string, bool> isValidFormat)
        {
            NumDiscardedValues = 0;

            // Using a HashSet here for two reasons:
            // 1. Get an implicit guarrantee that all values added to the collection are unique while maintaining O(1) insertion speed
            // 2. Minimize memory footprint required by keeping only the unique list of values in memory while parsing
            var uniqueValues = new HashSet<string>();

            using(var reader = new StreamReader(_filename))
            using (var csvReader = new CsvReader(reader, _hasHeaders))
            {
                // bail early if the number of fields is incorrect
                if (csvReader.FieldCount < fieldIndex)
                {
                    throw new CsvParsingException(string.Format("Input file has less than the expected number of fields {0}", fieldIndex));
                }

                // Raise exceptions whenever a parsing problem occurs or if expected data is missing. Default library behaviour is to return nulls.
                csvReader.DefaultParseErrorAction = ParseErrorAction.RaiseEvent;
                // Handle parsing errors with individual rows gracefully and continue reading
                csvReader.ParseError += csvReader_ParseError; // no need for explicit unsubscription since csvReader is guarranteed to be disposed
                
                while (csvReader.ReadNextRecord())
                {
                    string value = csvReader[fieldIndex]; // we get to handle out-of-bounds errors by subscribing to the ParseError event
                    if (isValidFormat(value))
                    {
                        uniqueValues.Add(value);
                    }
                    else
                    {
                        NumDiscardedValues++;
                    }
                }
            }

            return uniqueValues;
        }

        private void csvReader_ParseError(object sender, ParseErrorEventArgs e)
        {
            // skip over incomplete data rows, but let all other exceptions bubble up
            if (e.Error is MissingFieldCsvException)
            {
                _logger.Warn(string.Format("Skipping recordIndex={0} because of missing expected field at index {1}",
                    e.Error.CurrentRecordIndex, e.Error.CurrentFieldIndex));
                e.Action = ParseErrorAction.AdvanceToNextLine;
            }
            else
            {
                string msg = string.Format("Unrecoverable parsing problem encountered at filePosition={0} (recordIndex={0}, fieldIndex={1})",
                    e.Error.CurrentPosition, e.Error.CurrentRecordIndex, e.Error.CurrentFieldIndex);
                _logger.ErrorException(msg, e.Error);

                throw new CsvParsingException(msg, e.Error) { FilePosition = e.Error.CurrentPosition };
            }
        }
    }
}