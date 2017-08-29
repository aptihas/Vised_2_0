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
        // GET: Manager
        public ActionResult ManagerLK()
        {
            return View();
        }
    }
}