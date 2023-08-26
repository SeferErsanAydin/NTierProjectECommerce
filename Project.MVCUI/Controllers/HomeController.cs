using Project.BLL.DesignPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.MVCUI.Controllers
{
    public class HomeController : Controller
    {
        AppUserRepository _apRep;
        // GET: Home

        public HomeController()
        {
            _apRep = new AppUserRepository();
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(AppUser appUser)
        {
            AppUser catchedUser = _apRep.FirstOrDefault(x => x.UserName == appUser.UserName);
            if (catchedUser == null)
            {
                ViewBag.User = "User Not Found";
                return View();
            }

            string decrypted = DantexCrypt.DeCrypt(catchedUser.Password);

            if (appUser.Password == decrypted && catchedUser.Role == ENTITIES.Enums.UserRole.Admin)
            {
                if (!catchedUser.Active) return ControlActive();
                Session["admin"] = catchedUser;
                return RedirectToAction("CategoryList", "Category", new { area = "Admin" });
            }
            else if (catchedUser.Role == ENTITIES.Enums.UserRole.Member && appUser.Password == decrypted)
            {
                if (!catchedUser.Active) return ControlActive();
                Session["member"] = catchedUser;
                return RedirectToAction("ShoppingList", "Shopping");
            }
            ViewBag.User = "User not found";
            return View();
        }

        private ActionResult ControlActive()
        {
            ViewBag.User = "Please activate your account";
            return View("Login");
        }
    }
}