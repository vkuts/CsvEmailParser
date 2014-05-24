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
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [HandleError(ExceptionType = typeof(CsvParsingException), View = "ProcessingError")]
        public ActionResult Index(ContactListUpload upload)
        {
            if (upload.File.ContentLength > 0)
            {
                var filename = Path.GetFileName(upload.File.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/UploadBin"), filename);
                upload.File.SaveAs(path);

                try
                {
                    var valueExtractor = new CsvUniqueEmailExtractor(path);

                    IEnumerable<string> uniqueEmails = valueExtractor.GetUnique();
                    IEnumerable<string> alphaSortedEmails = uniqueEmails.OrderBy(e => e).ToList();

                    // TODO: use ajax instead of this lame temp data multi-view approach

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
            return View("DisplayResults", (IEnumerable<string>)TempData[TempDataEmailResult]);
        }
    }
}