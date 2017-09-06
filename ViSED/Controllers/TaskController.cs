using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED.ProgramLogic;

namespace ViSED.Controllers
{
    [ViSedRolesAttribute(Roles = "User")]
    public class TaskController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: Task
        public ActionResult TasksList()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var tasks = from t in vsdEnt.Tasks
                        where t.user_id_to == myAccount.user_id
                        select t;

            ViewBag.Tasks = tasks;
            ViewBag.Type = "zadachi";

            return View(tasks);
        }

        public ActionResult TasksListPartial(string taskVid)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            if (taskVid == "zadachi")
            {
                var tasks = from t in vsdEnt.Tasks
                            where t.user_id_to == myAccount.user_id
                            select t;

                ViewBag.Tasks = tasks;
                ViewBag.Type = "zadachi";
            }
            else if (taskVid == "porucheniya")
            {
                var tasks = from t in vsdEnt.Tasks
                            where t.user_id_from == myAccount.user_id
                            select t;


                ViewBag.Tasks = tasks;
                ViewBag.Type = "porucheniya";
            }

            return View();
        }
    }
}