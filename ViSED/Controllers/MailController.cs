using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ViSED.Controllers
{
    public class MailController : Controller
    {
        // GET: Mail
        public ActionResult IndexEmail()
        {
            return View();
        }

        public ActionResult NewEmail()
        {
            return View();
        }

        public ActionResult EmailList()
        {
            return View();
        }
    }
}