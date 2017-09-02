using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED.ProgramLogic;
using ViSED.Models;
using System.Web.Routing;
using NPetrovichLite;
using LingvoNET;
using System.Threading;
using System.Threading.Tasks;
using NReco.PdfGenerator;

namespace ViSED.Controllers
{

    [ViSedRolesAttribute(Roles = "User")]
    public class UserController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: User
        public ActionResult USerLK()//Личный кабинет
        {
            return View();
        }
        public ActionResult DocSelect()//Страница списка документов для выбора
        {
            var docs = from d in vsdEnt.DocType
                       select d;
            return View(docs);
        }
        public ActionResult DocData(int id)//Страница указания данных для документа (получатель и текст)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var myUser = (from u in vsdEnt.Users
                          where u.id == myAccount.user_id
                          select u).FirstOrDefault();

            var doc = (from d in vsdEnt.DocType
                       where d.id == id
                       select d).FirstOrDefault();

            var myDocs = from m in vsdEnt.MyDocs
                         where m.user_id == myAccount.user_id
                         select m;

            SelectList users;

            if (doc.Ierarhiya == true)
            {
                var usr = from u in vsdEnt.Users
                          from a in vsdEnt.Accounts
                          from r in vsdEnt.Roles
                          where u.id == a.user_id && a.role_id == r.id && r.RoleName == "User" && u.id != myUser.id && myAccount.Users.Dolgnosti.NomerIerarhii < u.Dolgnosti.NomerIerarhii
                          select new { id = u.id, Name = u.first_name + " " + u.second_name + " " + u.third_name };

                users = new SelectList(usr, "id", "Name");
            }
            else
            {
                var usr = from u in vsdEnt.Users
                          from a in vsdEnt.Accounts
                          from r in vsdEnt.Roles
                          where u.id == a.user_id && a.role_id == r.id && r.RoleName == "User" && u.id != myUser.id
                          select new { id = u.id, Name = u.first_name + " " + u.second_name + " " + u.third_name };

                users = new SelectList(usr, "id", "Name");
            }

            ViewBag.Users = users;
            ViewBag.MyDocs = myDocs;
            ViewBag.Doc = doc;
            return View();
        }

        public ActionResult DocSave(int[] user_to_id, int[] myDocs, string text, int doc_id, HttpPostedFileBase[] attachment)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var myUser = (from u in vsdEnt.Users
                          where u.id == myAccount.user_id
                          select u).FirstOrDefault();

            List<MyTask> msgList = new List<MyTask>();
            for (int i = 0; i < user_to_id.Length; i++)
            {
                MyTask msg = new MyTask
                {
                    from_user_id = myUser.id,
                    to_user_id = user_to_id[i],
                    doc_type_id = doc_id,
                    text = text,
                    dateOfSend = DateTime.Now,
                    dateOfRead = null
                };
                vsdEnt.MyTask.Add(msg);
                vsdEnt.SaveChanges();
                msgList.Add(msg);
            }

            if (attachment[0] != null)
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Attachments")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Attachments"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString())))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString()));
                }

                for (int i = 0; i < attachment.Length; i++)
                {
                    foreach (MyTask msg in msgList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(attachment[i].FileName);
                        string attachmnetName = System.IO.Path.GetFileName(attachment[i].FileName);
                        // сохраняем файл в папку Files в проекте
                        attachment[i].SaveAs(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension));
                        Attachments file = new Attachments { id_myTask = msg.id, attachedFile = "~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension, attachedName = attachmnetName };

                        vsdEnt.Attachments.Add(file);
                    }

                }
                vsdEnt.SaveChanges();
            }

            if (myDocs?.Length!=null && myDocs.Length > 0)
            {
                for (int i = 0; i < myDocs.Length; i++)
                {
                    int id_mydoc = myDocs[i];
                    var md = (from m in vsdEnt.MyDocs
                              where m.id == id_mydoc
                              select m).FirstOrDefault();

                    foreach (MyTask msg in msgList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(md.myDoc);
                        string attachmnetName = md.myDocName;
                        // сохраняем файл в папку Files в проекте
                        //attachment[i].SaveAs(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension));
                        Attachments file = new Attachments { id_myTask = msg.id, attachedFile = md.myDoc, attachedName = attachmnetName };

                        vsdEnt.Attachments.Add(file);
                    }
                }
                vsdEnt.SaveChanges();
            }
            //-------
            return RedirectToAction("DocView", "User", new { doc_id = msgList[0].id });
        }

        public ActionResult DocView(int doc_id)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var msg = (from m in vsdEnt.MyTask
                       where m.id == doc_id
                       select m).FirstOrDefault();

            var doc_type = (from d in vsdEnt.DocType
                            where msg.doc_type_id == d.id
                            select d).FirstOrDefault();

            var userFrom = (from u in vsdEnt.Users
                            where u.id == msg.from_user_id
                            select u).FirstOrDefault();

            var userTo = (from u in vsdEnt.Users
                          where u.id == msg.to_user_id
                          select u).FirstOrDefault();

            ViewBag.UserForNomer = userFrom;

            ViewBag.UserTo = SklonenieTo(userTo);
            ViewBag.UserFrom = SklonenieFrom(userFrom);
            ViewBag.DocType = doc_type;
            ViewBag.Msg = msg;

            //проверка относится ли пользователь к запрошиваемым документам. Если да то документ выводится
            if ((myAccount.user_id == msg.from_user_id) || (myAccount.user_id == msg.to_user_id))
            {
                return View(msg);
            }
            else
            {
                return RedirectToAction("USerLK", "User", null);
            }
        }
        [AllowAnonymous]
        public ActionResult DocViewPartial(int doc_id)
        {
            string controller = RouteData.GetRequiredString("controller");
            string action = RouteData.GetRequiredString("action");

            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var msg = (from m in vsdEnt.MyTask
                       where m.id == doc_id
                       select m).FirstOrDefault();

            var doc_type = (from d in vsdEnt.DocType
                            where msg.doc_type_id == d.id
                            select d).FirstOrDefault();

            var userFrom = (from u in vsdEnt.Users
                            where u.id == msg.from_user_id
                            select u).FirstOrDefault();

            var userTo = (from u in vsdEnt.Users
                          where u.id == msg.to_user_id
                          select u).FirstOrDefault();

            ViewBag.UserForNomer = userFrom;

            ViewBag.UserTo = SklonenieTo(userTo);
            ViewBag.UserFrom = SklonenieFrom(userFrom);
            ViewBag.DocType = doc_type;
            ViewBag.Msg = msg;
            try
            {
                //запросе неявно передается второй параметр saved который представляет с собой hashcode записи vised+doc_id
                //в методе проверяется данные переданные через параметрsaved с данными которые формируются точно также в методе если они совпадают то выдается страница если нет то нет
                if (Request.QueryString.GetValues(1)[0] == "vised" + doc_id.ToString().GetHashCode().ToString())
                {
                    return View();
                }
                else
                {
                    return View("NotFound");
                }
            }
            catch
            {
                return View("NotFound");
            }
        }

        public ActionResult CorrespondenceList()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var MyTask = from m in vsdEnt.MyTask
                          where m.to_user_id == myAccount.user_id
                          orderby m.dateOfSend descending
                          select m;
            ViewBag.MyTask = MyTask;
            ViewBag.Type = "vhod";

            return View();
        }
        [HttpPost]
        public ActionResult CorrespondenceListPartial(string vidSoobsh)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();
            if (vidSoobsh== "vhod")
            {
                 var MyTask = from m in vsdEnt.MyTask
                              where m.to_user_id == myAccount.user_id
                              orderby m.dateOfSend descending
                              select m;
                ViewBag.MyTask = MyTask;
                ViewBag.Type = "vhod";
            }
            else
            {
                var MyTask = from m in vsdEnt.MyTask
                              where m.from_user_id == myAccount.user_id
                              orderby m.dateOfSend descending
                              select m;
                ViewBag.MyTask = MyTask;
                ViewBag.Type = "ishod";
            }

            return PartialView();
        }

        public ActionResult Dialog(int userId, int msgCount)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var msgs = from m in vsdEnt.MyTask
                       where (m.from_user_id == userId && m.to_user_id == myAccount.user_id) || (m.to_user_id == userId && m.from_user_id == myAccount.user_id)
                       orderby m.id
                       select m;

            var attachments = from a in vsdEnt.Attachments
                              from m in msgs
                              where a.id_myTask == m.id
                              select a;

            ViewBag.Attachments = attachments;
            ViewBag.MyAccount = myAccount;
            if (msgCount <= msgs.Count())
            {
                ViewBag.MyTask = msgs.Skip<MyTask>(msgs.Count() - msgCount);
            }
            else
            {
                ViewBag.MyTask = msgs;
            }
            ViewBag.UserId = userId;
            ViewBag.MsgCount = msgCount;


            var msgAnswers = from m in vsdEnt.MyTask
                             where m.from_user_id == userId && m.to_user_id == myAccount.user_id && m.dateOfRead == null
                             select m;

            if (msgAnswers != null)
            {
                foreach (var m in msgAnswers)
                {
                    m.dateOfRead = DateTime.Now;
                }
                vsdEnt.SaveChanges();
            }

            return View();
        }
        [HttpPost]
        public ActionResult DialogPartial(int userId, int msgCount)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var msgs = from m in vsdEnt.MyTask
                       where (m.from_user_id == userId && m.to_user_id == myAccount.user_id) || (m.to_user_id == userId && m.from_user_id == myAccount.user_id)
                       orderby m.id
                       select m;

            var attachments = from a in vsdEnt.Attachments
                              from m in msgs
                              where a.id_myTask == m.id
                              select a;
            ViewBag.Attachments = attachments;
            if (msgCount <= msgs.Count())
            {
                ViewBag.MyTask = msgs.Skip<MyTask>(msgs.Count() - msgCount);
            }
            else
            {
                ViewBag.MyTask = msgs;
            }
            ViewBag.UserId = userId;
            ViewBag.MyAccount = myAccount;
            ViewBag.MsgCount = msgCount;

            var msgAnswers = from m in vsdEnt.MyTask
                             where m.from_user_id == userId && m.to_user_id == myAccount.user_id && m.dateOfRead == null
                             select m;

            if (msgAnswers != null)
            {
                foreach (var m in msgAnswers)
                {
                    m.dateOfRead = DateTime.Now;
                }
                vsdEnt.SaveChanges();
            }

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SavePdf(int doc_id)
        {
            var msg = (from m in vsdEnt.MyTask
                       where m.id == doc_id
                       select m).FirstOrDefault();

            string Host = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf("User") - 1);
            string Zapros = Url.Action("DocViewPartial", "User", new { doc_id = msg.id, saved = "vised" + msg.id.ToString().GetHashCode() });

            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter() { };
            htmlToPdf.Orientation = NReco.PdfGenerator.PageOrientation.Portrait;
            string put = Host + Zapros;
            byte[] pdfBytes = await HtmlToPdf(put, htmlToPdf);

            // return resulted pdf document 
            FileResult fileResult = new FileContentResult(pdfBytes, "application/pdf") { };
            fileResult.FileDownloadName = "Document_" + msg.id.ToString() + ".pdf";
            return fileResult;
        }

        private Task<byte[]> HtmlToPdf(string put, HtmlToPdfConverter htmlToPdf)
        {
            return Task.Run(() => htmlToPdf.GeneratePdfFromFile(put, null));
        }

        [AllowAnonymous]
        public UserModelFoSklonen SklonenieTo(Users user)
        {
            UserModelFoSklonen usr = new UserModelFoSklonen() { Dolgnost = user.Dolgnosti.Name, Podrazdelenie = user.Podrazdeleniya.Name, FirstName = user.first_name, SecondName = user.second_name, ThirdName = user.third_name, Blank = user.Podrazdeleniya.Blank };

            //При использованни этой библиотеки выдает ошибки
            usr.Dolgnost = DeclensionBLL.GetAppointmentDeclension(usr.Dolgnost, DeclensionCase.Datel);
            usr.Podrazdelenie = DeclensionBLL.GetOfficeDeclension(usr.Podrazdelenie, DeclensionCase.Rodit);

            Petrovich petrovich = new Petrovich();
            usr.FirstName = petrovich.Inflect(usr.FirstName, NamePart.LastName, NPetrovichLite.Case.Dative);
            usr.SecondName = petrovich.Inflect(usr.SecondName, NamePart.FirstName, NPetrovichLite.Case.Dative);
            usr.ThirdName = petrovich.Inflect(usr.ThirdName, NamePart.MiddleName, NPetrovichLite.Case.Dative);

            return usr;
        }

        [AllowAnonymous]
        public UserModelFoSklonen SklonenieFrom(Users user)
        {
            UserModelFoSklonen usr = new UserModelFoSklonen() { Dolgnost = user.Dolgnosti.Name, Podrazdelenie = user.Podrazdeleniya.Name, FirstName = user.first_name, SecondName = user.second_name, ThirdName = user.third_name, Blank = user.Podrazdeleniya.Blank };
            //При использованни этой библиотеки выдает ошибки
            usr.Dolgnost = DeclensionBLL.GetAppointmentDeclension(usr.Dolgnost, DeclensionCase.Rodit);
            usr.Podrazdelenie = DeclensionBLL.GetOfficeDeclension(usr.Podrazdelenie, DeclensionCase.Rodit);

            Petrovich petrovich = new Petrovich();
            usr.FirstName = petrovich.Inflect(usr.FirstName, NamePart.LastName, NPetrovichLite.Case.Genitive);
            usr.SecondName = petrovich.Inflect(usr.SecondName, NamePart.FirstName, NPetrovichLite.Case.Genitive);
            usr.ThirdName = petrovich.Inflect(usr.ThirdName, NamePart.MiddleName, NPetrovichLite.Case.Genitive);
            return usr;
        }
    }
}