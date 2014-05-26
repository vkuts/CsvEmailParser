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
    public class ContactsUploadController : Controller
    {
        private const string TempDataEmailResult = "result";
        //
        // GET: /ContactsUpload/
        public ActionResult DisplayView()
        {
            return View("UploadContacts");
        }

        [HttpPost]
        [HandleError(ExceptionType = typeof(CsvParsingException), View = "ProcessingError")]
        public ActionResult Upload(ContactListUpload upload)
        {
            if (upload.File.ContentLength > 0)
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

        [HttpGet]
        public ActionResult DisplayResults()
        {
            IEnumerable<string> results = new List<string>();
            if(TempData.ContainsKey(TempDataEmailResult)){
                results = (IEnumerable<string>)TempData[TempDataEmailResult];
            }
            return View("DisplayResults", results);
        }
    }
}