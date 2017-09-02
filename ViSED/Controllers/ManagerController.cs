using NReco.PdfGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ViSED.ProgramLogic;

namespace ViSED.Controllers
{
    [ViSedRolesAttribute(Roles = "Manager")]
    public class ManagerController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: Manager
        public ActionResult ManagerLK()
        {
            return View();
        }

        public ActionResult CorrespondenceJournal()
        {
            ViewBag.Period = "с " + DateTime.Now.ToShortDateString() + " по " + DateTime.Now.ToShortDateString();

            var msgs = from m in vsdEnt.MyTask
                       select m;

            ViewBag.MyTask = msgs;

            return View();
        }
 
        [AllowAnonymous]
        public ActionResult CorrespondenceJournalPartial(DateTime nachalo, DateTime konec)
        {
            DateTime _nachalo= DateTime.Now;
            DateTime _konec= DateTime.Now;
            
            if(DateTime.TryParse(nachalo.ToString(),out _nachalo) &&  DateTime.TryParse(konec.ToString(), out _konec))
            {
                var msgs = from m in vsdEnt.MyTask
                           where m.dateOfSend >= _nachalo && m.dateOfSend <= _konec
                           select m;

                ViewBag.MyTask = msgs;
                if (nachalo != konec)
                {
                    ViewBag.Period = "на период с " + _nachalo.ToShortDateString() + " по " + _konec.ToShortDateString();
                }
                else
                {
                    ViewBag.Period = "на " + _nachalo.ToShortDateString();
                }
            }
            else
            {
                var msgs = from m in vsdEnt.MyTask
                           where m.dateOfSend >= _nachalo && m.dateOfSend <= _konec
                           select m;

                ViewBag.MyTask = msgs;
                if (nachalo != konec)
                {
                    ViewBag.Period = "на период с " + _nachalo.ToShortDateString() + " по " + _konec.ToShortDateString();
                }
                else
                {
                    ViewBag.Period = "на " + _nachalo.ToShortDateString();
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SavePdf(DateTime nachaloHid, DateTime konecHid)
        {

            string Host = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf("Manager") - 1);
            string Zapros = Url.Action("CorrespondenceJournalPartial", "Manager", new { nachalo = nachaloHid, konec = konecHid });

            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter() { };
            htmlToPdf.Orientation = NReco.PdfGenerator.PageOrientation.Portrait;
            string put = Host + Zapros;
            byte[] pdfBytes = await HtmlToPdf(put, htmlToPdf);

            // return resulted pdf document 
            FileResult fileResult = new FileContentResult(pdfBytes, "application/pdf") { };
            fileResult.FileDownloadName = "Journal" + nachaloHid.ToShortDateString() + "--"+ konecHid.ToShortDateString() + ".pdf";
            return fileResult;
        }

        private Task<byte[]> HtmlToPdf(string put, HtmlToPdfConverter htmlToPdf)
        {
            return Task.Run(() => htmlToPdf.GeneratePdfFromFile(put, null));
        }

    }
}