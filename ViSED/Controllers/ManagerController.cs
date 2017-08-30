using System;
using System.Collections.Generic;
using System.Linq;
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

            var msgs = from m in vsdEnt.Message
                       select m;

            ViewBag.Message = msgs;

            return View();
        }
        [HttpPost]
        public ActionResult CorrespondenceJournalPartial(DateTime nachalo, DateTime konec)
        {
            DateTime _nachalo= DateTime.Now;
            DateTime _konec= DateTime.Now;
            
            if(DateTime.TryParse(nachalo.ToString(),out _nachalo) &&  DateTime.TryParse(konec.ToString(), out _konec))
            {
                var msgs = from m in vsdEnt.Message
                           where m.dateOfSend >= _nachalo && m.dateOfSend <= _konec
                           select m;

                ViewBag.Message = msgs;
                ViewBag.Period = "с " + _nachalo.ToShortDateString() + " по " + _konec.ToShortDateString();
            }
            else
            {
                var msgs = from m in vsdEnt.Message
                           where m.dateOfSend >= _nachalo && m.dateOfSend <= _konec
                           select m;

                ViewBag.Message = msgs;
                ViewBag.Period = "с " + _nachalo.ToShortDateString() + " по " + _konec.ToShortDateString();
            }
            return View();
        }

    }
}