using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ViSED.Controllers
{
    public class MyTaskController : Controller
    {
        // GET: MyTask
        public ActionResult MyTaskList()
        {
            return View();
        }
    }
}