using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ContactsUploader.Helpers
{
    /// <summary>
    /// Given a csv file and a column (field) index, extracts the set of unique string values in that column
    /// </summary>
    public class CsvUniqueValueExtractor
    {
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string _filename;
        private bool _hasHeaders;

        public int NumDiscardedValues { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="hasHeaders"></param>
        public CsvUniqueValueExtractor(string filename, bool hasHeaders)
        {
            _filename = filename;
            _hasHeaders = hasHeaders;
        }


        public IEnumerable<string> GetUnique(int fieldIndex)
        {
            return GetUnique(fieldIndex, (s) => true);
        }

        public IEnumerable<string> GetUnique(int fieldIndex, Func<string, bool> isValidFormat)
        {
            NumDiscardedValues = 0;

            // Using a HashSet here for two reasons:
            // 1. Get an implicit guarrantee that all emails added to the collection are unique while maintaining O(1) insertion speed
            // 2. Minimize memory footprint required by keeping only the unique list of emails in memory during parsing
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