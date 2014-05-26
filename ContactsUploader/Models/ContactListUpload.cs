using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContactsUploader.Models
{
    /// <summary>
    /// Represents a request to upload a csv file with contacts
    /// </summary>
    public class ContactListUpload
    {
        public HttpPostedFileBase File { get; set; }
    }
}