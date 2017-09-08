using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED.Models;
using ViSED.ProgramLogic;

namespace ViSED.Controllers
{
    [ViSedRolesAttribute(Roles = "User")]
    public class TaskController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: Task

        public ActionResult CreateTask()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            SelectList users;
            var usr = from u in vsdEnt.Users
                      from a in vsdEnt.Accounts
                      from r in vsdEnt.Roles
                      where u.id == a.user_id && a.role_id == r.id && r.RoleName == "User" && u.id != myAccount.user_id && myAccount.Users.Dolgnosti.NomerIerarhii < u.Dolgnosti.NomerIerarhii
                      select new { id = u.id, Name = u.first_name + " " + u.second_name + " " + u.third_name };

            users = new SelectList(usr, "id", "Name");
            ViewBag.Users = users;

            var myDocs = from m in vsdEnt.MyDocs
                         where m.user_id == myAccount.user_id
                         select m;
            ViewBag.MyDocs = myDocs;
            return View();
        }

        [HttpPost]
        public ActionResult CreateTask(int taskType, DateTime? deadline,int[] user_to, string taskName, string text, HttpPostedFileBase[] attachment, int[] myDocs)
        {
            //надо добавить возможность прикрепления файлов. Она пока не реализована
            DateTime? _deadline;
            if (taskType == 1)
            {
                _deadline = deadline;
            }
            else
            {
                _deadline = null;
            }

            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();
            int? _userFrom = null;
            List<int> poluchateli = new List<int>();
            if (user_to != null)
            {
                for (int i = 0; i < user_to.Length; i++)
                {
                    poluchateli.Add(user_to[i]);
                }
                poluchateli.Add(myAccount.user_id);
            }
            else
            {
                poluchateli.Add(myAccount.user_id);
            }

            List<Tasks> tskList = new List<Tasks>();
            for (int i = 0; i < poluchateli.Count; i++)
            {
                if (poluchateli[i] != myAccount.user_id)
                {
                    _userFrom = myAccount.user_id;
                }
                else
                {
                    _userFrom = null;
                }
                Models.Tasks _task = new Models.Tasks()
                {
                    id_letter = null,
                    complete = false,
                    dateOfCreate = DateTime.Now,
                    dateDeadline = _deadline,
                    taskName = taskName,
                    taskText = text,
                    taskType = taskType,
                    user_id_to = poluchateli[i],
                    user_id_from = _userFrom
                };
                vsdEnt.Tasks.Add(_task);
                vsdEnt.SaveChanges();
                tskList.Add(_task);
            }

            if (attachment[0] != null)
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/TaskAttachments")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/TaskAttachments"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/TaskAttachments/" + myAccount.user_id.ToString())))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/TaskAttachments/" + myAccount.user_id.ToString()));
                }

                for (int j = 0; j < attachment.Length; j++)
                {
                    foreach (Tasks tsk in tskList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(attachment[j].FileName);
                        string attachmnetName = System.IO.Path.GetFileName(attachment[j].FileName);
                        // сохраняем файл в папку Files в проекте
                        attachment[j].SaveAs(Server.MapPath("~/Files/TaskAttachments/" + myAccount.user_id.ToString() + "/file_" + tsk.id.ToString() + "_" + j.ToString() + extension));
                        TaskAttachments file = new TaskAttachments { id_task = tsk.id, attachedFile = "~/Files/TaskAttachments/" + myAccount.user_id.ToString() + "/file_" + tsk.id.ToString() + "_" + j.ToString() + extension, attachedName = attachmnetName };

                        vsdEnt.TaskAttachments.Add(file);
                    }

                }
                vsdEnt.SaveChanges();
            }

            if (myDocs?.Length != null && myDocs.Length > 0)
            {
                for (int i = 0; i < myDocs.Length; i++)
                {
                    int id_mydoc = myDocs[i];
                    var md = (from m in vsdEnt.MyDocs
                              where m.id == id_mydoc
                              select m).FirstOrDefault();

                    foreach (Tasks tsk in tskList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(md.myDoc);
                        string attachmnetName = md.myDocName;
                        // сохраняем файл в папку Files в проекте
                        TaskAttachments file = new TaskAttachments { id_task = tsk.id, attachedFile = md.myDoc, attachedName = attachmnetName };

                        vsdEnt.TaskAttachments.Add(file);
                    }
                }
                vsdEnt.SaveChanges();
            }

            return RedirectToAction("TasksList", "Task", null);
        }
        public ActionResult TasksList()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var tasks = from t in vsdEnt.Tasks
                        where t.user_id_to == myAccount.user_id
                        orderby t.dateOfCreate descending
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
                            orderby t.dateOfCreate descending
                            select t;

                ViewBag.Tasks = tasks;
                ViewBag.Type = "zadachi";
            }
            else if (taskVid == "porucheniya")
            {
                var tasks = from t in vsdEnt.Tasks
                            where t.user_id_from == myAccount.user_id
                            orderby t.dateOfCreate descending
                            select t;


                ViewBag.Tasks = tasks;
                ViewBag.Type = "porucheniya";
            }

            return View();
        }
    }
}