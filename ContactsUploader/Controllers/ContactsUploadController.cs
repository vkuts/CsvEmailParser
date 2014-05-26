using ContactsUploader.Helpers;
using ContactsUploader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContactsUploader.Controllers
{
    /// <summary>
    /// Uploads and displays contact lists
    /// </summary>
    public class ContactsUploadController : Controller
    {
        private const string TempDataEmailResult = "result";
        
        /// <summary>
        /// Displays upload contacts view
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplayView()
        {
            return View("UploadContacts");
        }

        /// <summary>
        /// Uploads contact list, processes it and redirects to the display view
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        [HttpPost]
        [HandleError(ExceptionType = typeof(CsvParsingException), View = "ProcessingError")]
        public ActionResult Upload(ContactListUpload upload)
        {
            if (upload.File != null && upload.File.ContentLength > 0)
            {
                var filename = Path.GetFileName(upload.File.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/UploadBin"), filename);
                upload.File.SaveAs(path);

                try
                {
                    var emailExtractor = new CsvUniqueEmailExtractor(path);

                    IEnumerable<string> uniqueEmails = emailExtractor.GetUnique();
                    IEnumerable<string> alphaSortedEmails = uniqueEmails.OrderBy(e => e).ToList();

                    // SHOULDDO: update current view with results via ajax rather than using TempData and redirecting

                    TempData[TempDataEmailResult] = alphaSortedEmails;

                }
                finally
                {
                    System.IO.File.Delete(path);
                }
            }

            return RedirectToAction("DisplayResults");
        }

        /// <summary>
        /// Displays a list of contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DisplayResults()
        {
            // Expects that the list of contacts is stored in TempData. This is currently only possible if the user gets here via Upload action.
            IEnumerable<string> results = new List<string>();
            if(TempData.ContainsKey(TempDataEmailResult)){
                results = (IEnumerable<string>)TempData[TempDataEmailResult];
            }
            return View("DisplayResults", results);
        }
    }
}