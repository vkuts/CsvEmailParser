using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ContactsUploader.Helpers
{
    /// <summary>
    /// Extracts the unique set of valid email addresses form the csv file
    /// </summary>
    /// <remarks>This class is not meant to be 100% reusable/generic. It encodes some assumtions about our particular csv input format
    /// (like which field index to find emails at, or the fact that we choose to discard invalid email strings)</remarks>
    public class CsvUniqueEmailExtractor
    {
        private const int EmailCsvFieldIndex = 2;

        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string _filename;
        private CsvUniqueValueExtractor _valueExtractor;
        private EmailFormatValidator _emailValidator = new EmailFormatValidator();

        public CsvUniqueEmailExtractor(string filename)
        {
            _filename = filename;
            _valueExtractor = new CsvUniqueValueExtractor(filename, true);
        }

        /// <summary>
        /// Extracts the unique set of valid email addresses from input file
        /// </summary>
        /// <remarks>Assumes that emails are located at a specific index</remarks>
        /// <returns></returns>
        public IEnumerable<string> GetUnique()
        {
            IEnumerable<string> result = _valueExtractor.GetUnique(EmailCsvFieldIndex, _emailValidator.IsValid);
            _logger.Debug(string.Format("{0}: extracted {1} emails, discarded {2} invalid emails.", _filename, result.Count(), _valueExtractor.NumDiscardedValues));
            return result;
        }
    }
}