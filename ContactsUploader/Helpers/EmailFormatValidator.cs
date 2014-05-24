using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ContactsUploader.Helpers
{
    /// <summary>
    /// Validates email address strings
    /// </summary>
    public class EmailFormatValidator
    {
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool IsValid(string email)
        {
            // check for null/empty value early because MailAddress would throw a different exception in those cases
            if (!String.IsNullOrWhiteSpace(email))
            {
                try
                {
                    var emailObj = new MailAddress(email);
                    return true;
                }
                catch (FormatException ex)
                {
                    _logger.TraceException(string.Format("Invalid email string encountered '{0}'", email), ex);
                    return false;
                }
            }
            return false;
        }
    }
}