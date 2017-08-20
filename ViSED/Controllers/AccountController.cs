using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ViSED.ProgramLogic;

namespace ViSED.Controllers
{
    public class AccountController : Controller
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        // GET: Account

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (ValidateUser(model.UserName, model.Password))
                {
                    var user = (from u in vsdEnt.Accounts
                                where u.login == model.UserName && u.passw == model.Password
                                select u).FirstOrDefault();

                    var userRole = (from ur in vsdEnt.Roles
                                    where ur.id == user.role_id
                                    select ur).FirstOrDefault();

                    FormsAuthentication.SetAuthCookie(model.UserName, true);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {

                        if(userRole.RoleName=="Admin")
                        {
                            return RedirectToAction("AdminLK", "Admin");
                        }
                        else if(userRole.RoleName == "User")
                        {
                            return RedirectToAction("UserLK", "User");
                        }
                        else
                        {
                            FormsAuthentication.SignOut();
                            return RedirectToAction("Login", "Account");
                        }
                    }
                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }


        private bool ValidateUser(string login, string password)
        {
            bool isValid = false;
            try
            {
                var user = (from u in vsdEnt.Accounts
                            where u.login == login && u.passw == password
                            select u).FirstOrDefault();
                if (user != null)
                {
                    isValid = true;
                }
            }
            catch
            {
                isValid = false;
            }
            return isValid;
        }
    }
}