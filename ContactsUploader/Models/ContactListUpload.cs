using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContactsUploader.Models
{
    public class ContactListUpload
    {
        public HttpPostedFileBase File { get; set; }
    }
}