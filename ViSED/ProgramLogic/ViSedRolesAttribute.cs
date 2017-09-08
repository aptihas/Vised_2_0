using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ViSED;
using System.Web.Security;
using System.IO;
using System.Text;

namespace ViSED.ProgramLogic
{
    public class ViSedRolesAttribute : AuthorizeAttribute
    {
        Models.ViSedDBEntities vsdEnt = new Models.ViSedDBEntities();
        private string allowedRoles = null;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (base.Roles!=null && base.Roles!="")
            {
                allowedRoles = base.Roles;
            }

            if (httpContext.Request.IsAuthenticated)
            {
             
                var usrAcc = (from l in vsdEnt.Accounts
                                where l.login == httpContext.User.Identity.Name
                                select l).FirstOrDefault();

                var rl = (from r in vsdEnt.Roles
                     where r.RoleName == allowedRoles
                     select r).FirstOrDefault();

                if (rl!=null && usrAcc!=null && rl.id==usrAcc.role_id)
                {
                    return true;
                }
                else
                {
                    var usrAdmin = (from l in vsdEnt.Admins
                                    where l.login == httpContext.User.Identity.Name
                                    select l).FirstOrDefault();

                    if (rl != null && usrAdmin != null && rl.id == usrAdmin.role_id)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
