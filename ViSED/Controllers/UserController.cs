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
using PagedList;
using System.IO;

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
            SoundTempDel();
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var myUser = (from u in vsdEnt.Users
                          where u.id == myAccount.user_id
                          select u).FirstOrDefault();
            ViewBag.MyUser = myUser;

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

        [HttpPost]
        public void SoundTemp(HttpPostedFileBase sound)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            if (myAccount.Users.Dolgnosti.UseAudio == true)
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Audio")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Audio"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString())))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString()));
                }
                // сохраняем файл в папку Files в проекте
                sound.SaveAs(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/file_sound_temp"));
            }
        }
        public void SoundTempDel()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();
            if (System.IO.Directory.Exists(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString())))
            {
                if(System.IO.File.Exists(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/file_sound_temp")))
                {
                    System.IO.File.Delete(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/file_sound_temp"));
                }
            }
        }

        [HttpPost]
        public ActionResult DocSave(int[] user_to_id, int[] myDocs, string text, int doc_id, HttpPostedFileBase[] attachment, int taskType,DateTime deadline)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var myUser = (from u in vsdEnt.Users
                          where u.id == myAccount.user_id
                          select u).FirstOrDefault();
            var docType = (from d in vsdEnt.DocType
                           where d.id == doc_id
                           select d).FirstOrDefault();



            List<Letters> msgList = new List<Letters>();
            for (int i = 0; i < user_to_id.Length; i++)
            {
                Letters msg = new Letters
                {
                    from_user_id = myUser.id,
                    to_user_id = user_to_id[i],
                    doc_type_id = doc_id,
                    text = text,
                    dateOfSend = DateTime.Now,
                    dateOfRead = null
                };
                vsdEnt.Letters.Add(msg);
                vsdEnt.SaveChanges();

                DateTime? _deadline = taskType == 1 ? deadline : (DateTime?)null;
                CreateTask(msg.id, myAccount.user_id, user_to_id[i], _deadline,msg.text, taskType,false, docType.Name);

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
                    foreach (Letters msg in msgList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(attachment[i].FileName);
                        string attachmnetName = System.IO.Path.GetFileName(attachment[i].FileName);
                        // сохраняем файл в папку Files в проекте
                        attachment[i].SaveAs(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension));
                        Attachments file = new Attachments {id_letter = msg.id, attachedFile = "~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension, attachedName = attachmnetName };

                        vsdEnt.Attachments.Add(file);
                    }

                }
                vsdEnt.SaveChanges();
            }


            //Голосовое сообщение
            //Голосое сообещние
            //Stream obj = Request.InputStream;
            //нужно просто при отправке формы вызывать функцию отправки аудио файла
            if (System.IO.File.Exists(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/file_sound_temp")))
            {
                System.IO.File.Move(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/file_sound_temp"), Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/audio_" + msgList[0].id.ToString() + ".webm"));
                string audioPathAbsolute = "~/Files/Audio/" + myAccount.user_id.ToString() + "/audio_" + msgList[0].id.ToString() + ".webm";
                System.IO.File.Delete(Server.MapPath("~/Files/Audio/" + myAccount.user_id.ToString() + "/file_sound_temp"));

                foreach (Letters msg in msgList)
                {
                    Audio _audio = new Audio { id_letter = msg.id, audioFile = audioPathAbsolute };
                    vsdEnt.Audio.Add(_audio);
                }

                vsdEnt.SaveChanges();
            }
            //------

            if (myDocs?.Length!=null && myDocs.Length > 0)
            {
                for (int i = 0; i < myDocs.Length; i++)
                {
                    int id_mydoc = myDocs[i];
                    var md = (from m in vsdEnt.MyDocs
                              where m.id == id_mydoc
                              select m).FirstOrDefault();

                    foreach (Letters msg in msgList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(md.myDoc);
                        string attachmnetName = md.myDocName;
                        // сохраняем файл в папку Files в проекте
                        //attachment[i].SaveAs(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension));
                        Attachments file = new Attachments { id_letter = msg.id, attachedFile = md.myDoc, attachedName = attachmnetName };

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

            var msg = (from m in vsdEnt.Letters
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
            msg.dateOfRead = DateTime.Now;
            vsdEnt.SaveChanges();

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

            var msg = (from m in vsdEnt.Letters
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

        public ActionResult CorrespondenceList(string vidSoobsh, int? page)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();
            ViewBag.Page = page ?? 1;

            if (vidSoobsh == "vhod")
            {
                var Letters = from m in vsdEnt.Letters
                              where m.to_user_id == myAccount.user_id
                              orderby m.dateOfSend descending
                              select m;
                ViewBag.Letters = Letters;
                ViewBag.Type = "vhod";

                int pageNumber = page ?? 1;
                int pageSize = 10;
                return View(Letters.ToList().ToPagedList(pageNumber, pageSize));
            }
            else
            {
                var Letters = from m in vsdEnt.Letters
                              where m.from_user_id == myAccount.user_id
                              orderby m.dateOfSend descending
                              select m;
                ViewBag.Letters = Letters;
                ViewBag.Type = "ishod";

                int pageNumber = page ?? 1;
                int pageSize = 10;
                return View(Letters.ToList().ToPagedList(pageNumber, pageSize));
            }
        }
        [HttpPost]
        public ActionResult CorrespondenceListPartial(string vidSoobsh, int? page)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();
            ViewBag.Page = page ?? 1;
            if (vidSoobsh== "vhod")
            {
                 var Letters = from m in vsdEnt.Letters
                              where m.to_user_id == myAccount.user_id
                              orderby m.dateOfSend descending
                              select m;
                ViewBag.Letters = Letters;
                ViewBag.Type = "vhod";
                int pageNumber = page ?? 1;
                int pageSize = 10;
                return PartialView(Letters.ToList().ToPagedList(pageNumber, pageSize));
            }
            else
            {
                var Letters = from m in vsdEnt.Letters
                              where m.from_user_id == myAccount.user_id
                              orderby m.dateOfSend descending
                              select m;
                ViewBag.Letters = Letters;
                ViewBag.Type = "ishod";

                int pageNumber = page ?? 1;
                int pageSize = 10;
                return PartialView(Letters.ToList().ToPagedList(pageNumber, pageSize));
            }


        }

        public ActionResult Dialog(int userId)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var msgs = from m in vsdEnt.Letters
                       where (m.from_user_id == userId && m.to_user_id == myAccount.user_id) || (m.to_user_id == userId && m.from_user_id == myAccount.user_id)
                       orderby m.id
                       select m;

            var attachments = from a in vsdEnt.Attachments
                              from m in msgs
                              where a.id_letter == m.id
                              select a;

            ViewBag.Attachments = attachments;
            ViewBag.MyAccount = myAccount;
            if (5 <= msgs.Count())
            {
                ViewBag.Letters = msgs.Skip<Letters>(msgs.Count() - 5);
            }
            else
            {
                ViewBag.Letters = msgs;
            }
            ViewBag.UserId = userId;

            var msgAnswers = from m in vsdEnt.Letters
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
        public ActionResult DialogPartial(int userId, int chisloSoobsh)
        {

            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var msgs = from m in vsdEnt.Letters
                       where (m.from_user_id == userId && m.to_user_id == myAccount.user_id) || (m.to_user_id == userId && m.from_user_id == myAccount.user_id)
                       orderby m.id
                       select m;

            var attachments = from a in vsdEnt.Attachments
                              from m in msgs
                              where a.id_letter == m.id
                              select a;

            ViewBag.Attachments = attachments;
            if (chisloSoobsh <= msgs.Count())
            {
                ViewBag.Letters = msgs.Skip<Letters>(msgs.Count() - chisloSoobsh);
            }
            else
            {
                ViewBag.Letters = msgs;
            }
            ViewBag.UserId = userId;
            ViewBag.MyAccount = myAccount;

            var msgAnswers = from m in vsdEnt.Letters
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
            var msg = (from m in vsdEnt.Letters
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

        public void CreateTask(int? _id_letter, int? _user_id_from, int _user_id_to, DateTime? _deadLine, string _taskText, int _taskType, bool _compleate, string _taskName)
        {
            Models.Accounts myAccount = (from u in vsdEnt.Accounts
                                         where u.login == User.Identity.Name
                                         select u).FirstOrDefault();

            Models.Tasks _task = new Models.Tasks()
            {
                id_letter = _id_letter ?? null,
                user_id_from = _user_id_from ?? null,
                user_id_to = _user_id_to,
                dateOfCreate = DateTime.Now,
                dateDeadline = _deadLine ?? null,
                taskText = _taskText,
                taskType = _taskType,
                complete = _compleate,
                taskName = _taskName
            };
            vsdEnt.Tasks.Add(_task);
            vsdEnt.SaveChanges();

        }
    }
}