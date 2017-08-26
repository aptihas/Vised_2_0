﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED.ProgramLogic;
using ViSED.Models;

namespace ViSED.Controllers
{
    [ViSedRolesAttribute(Roles = "User")]
    public class UnsubscribeController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: Unsubscribe
        public ActionResult UnsubcribeData()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var myUser = (from u in vsdEnt.Users
                          where u.id == myAccount.user_id
                          select u).FirstOrDefault();

            var myDocs = from m in vsdEnt.MyDocs
                         where m.user_id == myAccount.user_id
                         select m;

            var usr = from u in vsdEnt.Users
                      from a in vsdEnt.Accounts
                      from r in vsdEnt.Roles
                      where u.id == a.user_id && a.role_id == r.id && r.RoleName == "User" && u.id != myUser.id
                      select new { id = u.id, Name = u.first_name + " " + u.second_name + " " + u.third_name };

            SelectList users = new SelectList(usr, "id", "Name");
            ViewBag.Users = users;
            ViewBag.MyDocs = myDocs;

            return View();
        }

        [HttpPost]
        public ActionResult UnsubcribeData(int[] user_to_id, HttpPostedFileBase[] attachment, int[] myDocs, DateTime date_of_unsub)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            List<Unsubscribe> uscList = new List<Unsubscribe>();
            for (int i = 0; i < user_to_id.Length; i++)
            {
                Unsubscribe usc = new Unsubscribe
                {
                    from_user_id = myAccount.user_id,
                    to_user_id = user_to_id[i],
                    date_of_unsub = DateTime.Now,
                    date_of_execution = date_of_unsub,
                    executed = false

                };
                vsdEnt.Unsubscribe.Add(usc);
                vsdEnt.SaveChanges();
                uscList.Add(usc);
            }
            if (attachment[0] != null && myDocs?.Length == null || myDocs.Length == 0)
            {
                return RedirectToAction("UnsubcribeSelect", "Unsubscribe", null);
            }
            if (attachment[0] != null)
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/UnsubscribeAttachments")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/UnsubscribeAttachments"));
                }

                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/UnsubscribeAttachments/" + myAccount.user_id.ToString())))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/UnsubscribeAttachments/" + myAccount.user_id.ToString()));
                }

                for (int i = 0; i < attachment.Length; i++)
                {
                    foreach (Unsubscribe usc in uscList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(attachment[i].FileName);
                        string attachmnetName = System.IO.Path.GetFileName(attachment[i].FileName);
                        // сохраняем файл в папку Files в проекте
                        attachment[i].SaveAs(Server.MapPath("~/Files/UnsubscribeAttachments/" + myAccount.user_id.ToString() + "/file_" + usc.id.ToString() + "_" + i.ToString() + extension));
                        UnsubAttachments file = new UnsubAttachments { id_unsubcribe = usc.id, attachedFile = "~/Files/UnsubscribeAttachments/" + myAccount.user_id.ToString() + "/file_" + usc.id.ToString() + "_" + i.ToString() + extension, attachedName = attachmnetName };

                        vsdEnt.UnsubAttachments.Add(file);
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

                    foreach (Unsubscribe msg in uscList)
                    {
                        //обработка приложения
                        string extension = System.IO.Path.GetExtension(md.myDoc);
                        string attachmnetName = md.myDocName;
                        // сохраняем файл в папку Files в проекте
                        //attachment[i].SaveAs(Server.MapPath("~/Files/Attachments/" + myUser.id.ToString() + "/file_" + msg.id.ToString() + "_" + i.ToString() + extension));
                        UnsubAttachments file = new UnsubAttachments { id_unsubcribe = msg.id, attachedFile = md.myDoc, attachedName = attachmnetName };

                        vsdEnt.UnsubAttachments.Add(file);
                    }
                }
                vsdEnt.SaveChanges();
            }

            return RedirectToAction("UnsubcribeSelect", "Unsubscribe", null);
        }
        public ActionResult UnsubcribeSelect()
        {
            return View();
        }

        public ActionResult UnsubcribeIn(int userId)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var user = (from u in vsdEnt.Users
                        where u.id == userId
                        select u).FirstOrDefault();

            var usc = from u in vsdEnt.Unsubscribe
                      where u.to_user_id == myAccount.user_id && u.from_user_id==user.id
                      select u;

            var uscAttachs = from u in usc
                             from ua in vsdEnt.UnsubAttachments
                             where ua.id_unsubcribe == u.id
                             select ua;

            ViewBag.UscAttachs = uscAttachs;

            return View(usc);
        }

        public ActionResult UnsubcribeOut(int userId)
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var user = (from u in vsdEnt.Users
                        where u.id == userId
                        select u).FirstOrDefault();

            var usc = from u in vsdEnt.Unsubscribe
                      where u.from_user_id == myAccount.user_id && u.to_user_id==user.id
                      select u;

            var uscAttachs = from u in usc
                             from ua in vsdEnt.UnsubAttachments
                             where ua.id_unsubcribe == u.id
                             select ua;

            ViewBag.UscAttachs = uscAttachs;

            return View(usc);
        }
        public ActionResult EndSubscribe(int id)
        {
            var usb = (from u in vsdEnt.Unsubscribe
                       where u.id == id
                       select u).FirstOrDefault();
            if (usb.executed == true)
            {
                usb.executed = false;
            }
            else
            {
                usb.executed = true;
            }
            vsdEnt.SaveChanges();
            return RedirectToAction("UnsubcribeIn", "Unsubscribe", null);
        }

        public ActionResult SubscribeCorrespondence()
        {
            var myAccount = (from u in vsdEnt.Accounts
                             where u.login == User.Identity.Name
                             select u).FirstOrDefault();

            var idFrom = from p in vsdEnt.Unsubscribe
                         where p.from_user_id != myAccount.user_id && p.to_user_id == myAccount.user_id
                         select p.from_user_id;

            var idTo = from p in vsdEnt.Unsubscribe
                       where p.to_user_id != myAccount.user_id && p.from_user_id == myAccount.user_id
                       select p.to_user_id;

            var idList = idFrom.Union<int>(idTo).Distinct();

            var userList = from u in vsdEnt.Users
                           from i in idList
                           where u.id == i
                           select u;

            var msgToMe = from m in vsdEnt.Unsubscribe
                          where m.to_user_id == myAccount.Users.id && m.executed == false
                          select m;

            ViewBag.UserList = userList;

            return View();
        }

        public ActionResult SubscribeUser(int userId)
        {
            var user= (from u in vsdEnt.Users
                      where u.id==userId
                      select u).FirstOrDefault();

            ViewBag.User = user;
            return View();
        }
    }
}