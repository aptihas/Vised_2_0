using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED.Models;
using System.Web.Routing;
using ViSED.ProgramLogic;
using PagedList;

namespace ViSED.Controllers
{
    [ViSedRolesAttribute(Roles = "User")]
    public class MyDocsController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: MyDocs
        public ActionResult MyDocsList(int? page)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var myDocs = from m in vsdEnt.MyDocs
                         where m.user_id == myAccount.user_id
                         select m;
            ViewBag.Page = page ?? 1;
            int pageNumber = page ?? 1;
            int pageSize = 10;
            return View(myDocs.ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult AddDoc()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddDoc(HttpPostedFileBase[] myDocs)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var max = from m in vsdEnt.MyDocs
                       where m.user_id == myAccount.user_id
                       orderby m.id
                       select m;

            int max_id=1;

            if (max.Count() != 0)
            {
                max_id = max.Max(m => m.id);
            }
          
            if (myDocs != null)
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/MyDocs")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/MyDocs"));
                }
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/MyDocs/" + myAccount.user_id.ToString())))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/MyDocs/" + myAccount.user_id.ToString()));
                }


                for (int i = 0; i < myDocs.Length; i++)
                {
                    max_id++;
                    //обработка приложения
                    string extension = System.IO.Path.GetExtension(myDocs[i].FileName);
                    string myDocName = System.IO.Path.GetFileName(myDocs[i].FileName);
                    // сохраняем файл в папку Files в проекте
                    myDocs[i].SaveAs(Server.MapPath("~/Files/MyDocs/" + myAccount.user_id.ToString() + "/myDoc_" + max_id + "_" + i.ToString() + extension));
                    MyDocs myDoc = new MyDocs { user_id = myAccount.user_id, myDoc = "~/Files/MyDocs/" + myAccount.user_id.ToString() + "/myDoc_" + max_id + "_" + i.ToString() + extension, myDocName = myDocName };

                    vsdEnt.MyDocs.Add(myDoc);
                    
                }
                vsdEnt.SaveChanges();

            }
            return RedirectToAction("MyDocsList", "MyDocs");
        }


        //удаление документа
        public ActionResult DocDelete(int id)
        {
            var myDoc = (from d in vsdEnt.MyDocs
                       where d.id == id
                       select d).FirstOrDefault();

            return View(myDoc);
        }

        [HttpPost]
        public ActionResult DocDelete(Models.MyDocs model)
        {
            var myDoc = (from d in vsdEnt.MyDocs
                       where d.id == model.id
                       select d).FirstOrDefault();
            System.IO.File.Delete(Server.MapPath(myDoc.myDoc));

            vsdEnt.MyDocs.Remove(myDoc);
            vsdEnt.SaveChanges();
            return RedirectToAction("MyDocsList", "MyDocs");
        }


    }
}