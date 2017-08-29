using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED.ProgramLogic;

namespace ViSED.Controllers
{
    [ViSedRolesAttribute(Roles = "Admin")]
    public class AdminController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: Admin
        public ActionResult AdminLK()
        {
            return View();
        }

        //Добавить подразделение
        //При добавлении подразделения нужно соблюдать иерархию. Добавлять надо начиная с главного подразделения по снипадающей
        public ActionResult AddPodrazdelenie()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPodrazdelenie(Models.Podrazdeleniya model, HttpPostedFileBase Blank)
        {
            if (model.Name != "" && model.Name != null && model.Blank != null && model.Blank != "")
            {
                vsdEnt.Podrazdeleniya.Add(model);
                vsdEnt.SaveChanges();
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/BlanksOfPodrazdeleni")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/BlanksOfPodrazdeleni"));
                }
                // получаем имя файла
                string extension = System.IO.Path.GetExtension(Blank.FileName);
                // сохраняем файл в папку Files в проекте
                Blank.SaveAs(Server.MapPath("~/Files/BlanksOfPodrazdeleni/" + "Blank_" + model.id.ToString() + extension));

                var ob = (from p in vsdEnt.Podrazdeleniya
                          where p.id == model.id
                          select p).FirstOrDefault();

                ob.Blank = "~/Files/BlanksOfPodrazdeleni/" + "Blank_" + model.id.ToString() + extension;
                vsdEnt.SaveChanges();

            }
            return RedirectToAction("ListPodrazdelenie", "Admin");
        }


        public ActionResult ListPodrazdelenie()
        {
            //Находим подразделения пользователей
            var podrazdels = from p in vsdEnt.Podrazdeleniya
                             select p;

            return View(podrazdels);
        }


        //необходимо сделать удаление и изменение подразделения вместе с бланком
        public ActionResult EditPodrazdelenie(int id)
        {
            var podrazdel = (from d in vsdEnt.Podrazdeleniya
                             where d.id == id
                             select d).FirstOrDefault();

            return View(podrazdel);
        }
        [HttpPost]
        public ActionResult EditPodrazdelenie(Models.Podrazdeleniya model, HttpPostedFileBase Blank)
        {
            var podrazdel = (from d in vsdEnt.Podrazdeleniya
                             where d.id == model.id
                             select d).FirstOrDefault();

            podrazdel.Name = model.Name;
            if (model.Blank != null && model.Blank != "")
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/BlanksOfPodrazdeleni")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/BlanksOfPodrazdeleni"));
                }
                // получаем имя файла
                string extension = System.IO.Path.GetExtension(Blank.FileName);
                // сохраняем файл в папку Files в проекте/ если файл с таким же именем есть то он перезаписывается
                System.IO.File.Delete(Server.MapPath("~/Files/BlanksOfPodrazdeleni/" + "Blank_" + podrazdel.id.ToString() + extension));
                Blank.SaveAs(Server.MapPath("~/Files/BlanksOfPodrazdeleni/" + "Blank_" + podrazdel.id.ToString() + extension));

                var ob = (from p in vsdEnt.Podrazdeleniya
                          where p.id == model.id
                          select p).FirstOrDefault();

                ob.Blank = "~/Files/BlanksOfPodrazdeleni/" + "Blank_" + model.id.ToString() + extension;
                vsdEnt.SaveChanges();
            }
            vsdEnt.SaveChanges();

            return RedirectToAction("ListPodrazdelenie", "Admin");
        }

        public ActionResult DelPodrazdelenie(int id)
        {
            var podrazdel = (from d in vsdEnt.Podrazdeleniya
                             where d.id == id
                             select d).FirstOrDefault();

            return View(podrazdel);
        }
        [HttpPost]
        public ActionResult DelPodrazdelenie(Models.Podrazdeleniya model)
        {
            var podrazdel = (from d in vsdEnt.Podrazdeleniya
                             where d.id == model.id
                             select d).FirstOrDefault();

            vsdEnt.Podrazdeleniya.Remove(podrazdel);

            // получаем имя файла
            string extension = System.IO.Path.GetExtension(podrazdel.Blank);
            // удаляем файл из папки BlanksOfPodrazdeleni
            try
            {
                System.IO.File.Delete(Server.MapPath("~/Files/BlanksOfPodrazdeleni/" + "Blank_" + podrazdel.id.ToString() + extension));
            }
            finally
            {
                vsdEnt.SaveChanges();
            }
            return RedirectToAction("ListPodrazdelenie", "Admin");
        }

        //Управление документами
        public ActionResult AddDocType()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddDocType(Models.DocType model)
        {
            if (model.Name != "" && model.Name != null)
            {
                vsdEnt.DocType.Add(model);
                vsdEnt.SaveChanges();
            }
            return RedirectToAction("ListDocType", "Admin");
        }

        public ActionResult ListDocType()
        {
            var docs = from d in vsdEnt.DocType
                       select d;
            ViewBag.Docs = docs;

            return View(docs);
        }

        public ActionResult EditDocType(int id)
        {
            var doc = (from d in vsdEnt.DocType
                       where d.id == id
                       select d).FirstOrDefault();

            return View(doc);
        }
        [HttpPost]
        public ActionResult EditDocType(Models.DocType model)
        {
            var doc = (from d in vsdEnt.DocType
                       where d.id == model.id
                       select d).FirstOrDefault();

            doc.Name = model.Name;
            doc.Use_Blank = model.Use_Blank;
            vsdEnt.SaveChanges();
            return RedirectToAction("ListDocType", "Admin");
        }

        public ActionResult DelDocType(int id)
        {
            var doc = (from d in vsdEnt.DocType
                       where d.id == id
                       select d).FirstOrDefault();

            return View(doc);
        }
        [HttpPost]
        public ActionResult DelDocType(Models.DocType model)
        {
            var doc = (from d in vsdEnt.DocType
                       where d.id == model.id
                       select d).FirstOrDefault();

            vsdEnt.DocType.Remove(doc);
            vsdEnt.SaveChanges();
            return RedirectToAction("ListDocType", "Admin");
        }




        //Добавить должность
        //При добавлении должности нужно соблюдать иерархию. Добавлять надо начиная с главного подразделения по снипадающей
        //Управление дожностями
        public ActionResult AddDolgnost()
        {
            var dolgnosti = from d in vsdEnt.Dolgnosti
                            select d;

            ViewBag.Dolgnosti = dolgnosti;
            return View();
        }

        [HttpPost]
        public ActionResult AddDolgnost(Models.Dolgnosti model)
        {
            if (model.Name != "" && model.Name != null)
            {
                vsdEnt.Dolgnosti.Add(model);
                vsdEnt.SaveChanges();
            }
            return RedirectToAction("ListDolgnost", "Admin");
        }

        public ActionResult ListDolgnost()
        {
            var dolgnosti = from d in vsdEnt.Dolgnosti
                            select d;

            ViewBag.Dolgnosti = dolgnosti;

            return View();
        }

        public ActionResult EditDolgnost(int id)
        {

            var dolgnosti = (from d in vsdEnt.Dolgnosti
                             where d.id == id
                             select d).FirstOrDefault();

            return View(dolgnosti);
        }
        [HttpPost]
        public ActionResult EditDolgnost(Models.Dolgnosti model)
        {
            var _dolgnosti = (from d in vsdEnt.Dolgnosti
                              where d.id == model.id
                              select d).FirstOrDefault();

            _dolgnosti.Name = model.Name;
            vsdEnt.SaveChanges();
            return RedirectToAction("ListDolgnost", "Admin");
        }

        public ActionResult DelDolgnost(int id)
        {
            var dolgnosti = (from d in vsdEnt.Dolgnosti
                             where d.id == id
                             select d).FirstOrDefault();

            return View(dolgnosti);
        }
        [HttpPost]
        public ActionResult DelDolgnost(Models.Dolgnosti model)
        {
            var dolgnosti = (from d in vsdEnt.Dolgnosti
                             where d.id == model.id
                             select d).FirstOrDefault();

            vsdEnt.Dolgnosti.Remove(dolgnosti);
            vsdEnt.SaveChanges();
            return RedirectToAction("ListDolgnost", "Admin");
        }

        //Добавить пользователя
        public ActionResult AddUser()
        {
            var podrazdeleniya = from p in vsdEnt.Podrazdeleniya
                                 select p;

            SelectList podrazdeleniyaSL = new SelectList(podrazdeleniya, "id", "Name");
            ViewBag.Podrazdeleniya = podrazdeleniyaSL;

            var dolgnosti = from d in vsdEnt.Dolgnosti
                            select d;

            SelectList dolgnostiSL = new SelectList(dolgnosti, "id", "Name");
            ViewBag.Dolgnosti = dolgnostiSL;

            return View();
        }

        [HttpPost]
        public ActionResult AddUser(Models.Users model, HttpPostedFileBase foto)
        {
            if (model.first_name != "" && model.second_name != "")
            {
                model.foto = "null";
                vsdEnt.Users.Add(model);
                vsdEnt.SaveChanges();
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                }
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Users")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Users"));
                }
                if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Users/Foto")))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Users/Foto"));
                }
                // получаем имя файла
                string extension = System.IO.Path.GetExtension(foto.FileName);
                // сохраняем файл в папку Files в проекте
                foto.SaveAs(Server.MapPath("~/Files/Users/Foto/" + "Foto_user_" + model.id.ToString() + extension));

                var ob = (from p in vsdEnt.Users
                          where p.id == model.id
                          select p).FirstOrDefault();

                ob.foto = "~/Files/Users/Foto/" + "Foto_user_" + model.id.ToString() + extension;
                vsdEnt.SaveChanges();

                GenerateAccount(model.id);
            }

            return RedirectToAction("ListUser", "Admin");
        }
        //Список пользователей
        public ActionResult ListUser()
        {
            var users = from u in vsdEnt.Users
                        from a in vsdEnt.Accounts
                        from r in vsdEnt.Roles
                        where u.id == a.user_id && a.role_id == r.id && r.RoleName == "User"
                        select u;

            ViewBag.Users = users;

            return View();
        }

        public ActionResult ListAccounts()
        {
            var role = (from r in vsdEnt.Roles
                        where r.RoleName == "Admin"
                        select r).FirstOrDefault();

            var accounts = from a in vsdEnt.Accounts
                           from u in vsdEnt.Users
                           from d in vsdEnt.Dolgnosti
                           from p in vsdEnt.Podrazdeleniya
                           where a.role_id != role.id && a.user_id == u.id && u.podrazdelenie_id == p.id && u.dolgnst_id == d.id
                           select new AccountListModel { podrazdelenie = p.Name, dolgnst = d.Name, first_name = u.first_name, second_name = u.second_name, third_name = u.third_name, login = a.login, password = a.passw };

            ViewBag.Accounts = accounts;

            return View();
        }

        [HttpPost]
        public ActionResult AccountSearch(string name)
        {
            var role = (from r in vsdEnt.Roles
                        where r.RoleName == "Admin"
                        select r).FirstOrDefault();


            if (name != "")
            {
                var accounts = from a in vsdEnt.Accounts
                               from u in vsdEnt.Users
                               from d in vsdEnt.Dolgnosti
                               from p in vsdEnt.Podrazdeleniya
                               where a.role_id != role.id && a.user_id == u.id && u.podrazdelenie_id == p.id && u.dolgnst_id == d.id && (u.first_name == name || u.second_name == name)
                               select new AccountListModel { podrazdelenie = p.Name, dolgnst = d.Name, first_name = u.first_name, second_name = u.second_name, third_name = u.third_name, login = a.login, password = a.passw };

                ViewBag.Accounts = accounts;
            }
            else
            {
                var accounts = from a in vsdEnt.Accounts
                               from u in vsdEnt.Users
                               from d in vsdEnt.Dolgnosti
                               from p in vsdEnt.Podrazdeleniya
                               where a.role_id != role.id && a.user_id == u.id && u.podrazdelenie_id == p.id && u.dolgnst_id == d.id
                               select new AccountListModel { podrazdelenie = p.Name, dolgnst = d.Name, first_name = u.first_name, second_name = u.second_name, third_name = u.third_name, login = a.login, password = a.passw };
                ViewBag.Accounts = accounts;
            }
            return PartialView();
        }

        //Изменить пользователя
        public ActionResult EditUser(int id)
        {
            var podrazdeleniya = from p in vsdEnt.Podrazdeleniya
                                 select p;

            SelectList podrazdeleniyaSL = new SelectList(podrazdeleniya, "id", "Name");
            ViewBag.Podrazdeleniya = podrazdeleniyaSL;

            var dolgnosti = from d in vsdEnt.Dolgnosti
                            select d;

            SelectList dolgnostiSL = new SelectList(dolgnosti, "id", "Name");
            ViewBag.Dolgnosti = dolgnostiSL;

            var user = (from u in vsdEnt.Users
                        where u.id == id
                        select u).FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public ActionResult EditUser(Models.Users model, HttpPostedFileBase foto)
        {
            if (model.first_name != "" && model.second_name != "")
            {
                var user = (from p in vsdEnt.Users
                          where p.id == model.id
                          select p).FirstOrDefault();

                if (foto != null)
                {
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files"));
                    }
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Users")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Users"));
                    }
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/Users/Foto")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/Users/Foto"));
                    }

                        // получаем имя файла
                        string extension = System.IO.Path.GetExtension(foto.FileName);
                        // сохраняем файл в папку Files в проекте
                        System.IO.File.Delete(Server.MapPath("~/Files/Users/Foto/" + "Foto_user_" + model.id.ToString() + extension));

                        foto.SaveAs(Server.MapPath("~/Files/Users/Foto/" + "Foto_user_" + model.id.ToString() + extension));
                        user.foto = "~/Files/Users/Foto/" + "Foto_user_" + model.id.ToString() + extension;
                }

                


                user.first_name = model.first_name;
                user.second_name = model.second_name;
                user.third_name = model.third_name;
                user.podrazdelenie_id = model.podrazdelenie_id;
                user.dolgnst_id = model.dolgnst_id;
                user.indexNum = model.indexNum;
                

                vsdEnt.SaveChanges();
            }

            return RedirectToAction("ListUser", "Admin");
        }

        //удалить пользователя
        public ActionResult DeleteUser(int id)
        {
            var podrazdeleniya = from p in vsdEnt.Podrazdeleniya
                                 select p;

            SelectList podrazdeleniyaSL = new SelectList(podrazdeleniya, "id", "Name");
            ViewBag.Podrazdeleniya = podrazdeleniyaSL;

            var dolgnosti = from d in vsdEnt.Dolgnosti
                            select d;

            SelectList dolgnostiSL = new SelectList(dolgnosti, "id", "Name");
            ViewBag.Dolgnosti = dolgnostiSL;

            var user = (from u in vsdEnt.Users
                        where u.id == id
                        select u).FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public ActionResult DeleteUser(Models.Users model)
        {
            var user = (from u in vsdEnt.Users
                        where u.id == model.id
                        select u).FirstOrDefault();
            var account = (from a in vsdEnt.Accounts
                           where a.user_id == model.id
                           select a).FirstOrDefault();

            System.IO.File.Delete(Server.MapPath(model.foto));

            vsdEnt.Accounts.Remove(account);
            vsdEnt.Users.Remove(user);
            vsdEnt.SaveChanges();
            return RedirectToAction("ListUser", "Admin");
        }

        [HttpPost]
        public ActionResult UserSearch(string name)
        {
            if (name != "")
            {
                var usrs = from u in vsdEnt.Users
                           select u;
                ViewBag.Users = usrs;
            }
            else
            {
                var usrs = from u in vsdEnt.Users
                           select u;
                ViewBag.Users = usrs;
            }
            return PartialView();
        }
        public void GenerateAccount(int user_id)
        {
            var role = (from r in vsdEnt.Roles
                        where r.RoleName == "User"
                        select r).FirstOrDefault();

            var loginList = from l in vsdEnt.Accounts
                            select l.login;
            string _login = GetLogin(6), _password = GetPass(8);
            while (loginList.Contains(_login))
            {
                _login = GetLogin(6);
            }
            var account = new Models.Accounts { role_id = role.id, user_id = user_id, login = _login, passw = _password };
            vsdEnt.Accounts.Add(account);
            vsdEnt.SaveChanges();
        }
        private string GetPass(int x)
        {
            string pass = "";
            var r = new Random();
            while (pass.Length < x)
            {
                Char c = (char)r.Next(33, 125);
                if (Char.IsLetterOrDigit(c))
                    pass += c;
            }
            return pass;
        }

        private string GetLogin(int x)
        {
            string login = "";
            var r = new Random();
            while (login.Length < x)
            {
                login += r.Next(0, 9).ToString();

            }
            return login;
        }
    }
}