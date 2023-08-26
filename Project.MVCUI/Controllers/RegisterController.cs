using Microsoft.Ajax.Utilities;
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
    public class RegisterController : Controller
    {

        AppUserRepository _apRep;
        ProfileRepository _proRep;

        public RegisterController()
        {
            _apRep = new AppUserRepository();
            _proRep = new ProfileRepository();
        }
        public ActionResult RegisterNow()
        {
            return View();
        }
        [HttpPost]

        public ActionResult RegisterNow(AppUser appUser, AppUserProfile userProfile)
        {
            appUser.Password = DantexCrypt.Crypt(appUser.Password); //password crypted

            if (_apRep.Any(x => x.UserName == appUser.UserName))
            {
                ViewBag.ZatenVar = "User name already exist. Please chose another.";
                return View();
            }
            else if (_apRep.Any(x => x.Email == appUser.Email))
            {
                ViewBag.ZatenVar = "Email already exists.";
                return View();
            }

            //after the user succesfully registers we will send an email for validation
            //https://localhost:44361/

            string registerEmail = "Congratulations! Your Account has been set. For activation please click the link https://localhost:44361/Register/Activation/" + appUser.ActivationCode;

            MailService.Send(appUser.Email, body: registerEmail, subject: "Account Activation");
            _apRep.Add(appUser); // due to relation between account and profile we need to add appuser data before its profile

            //white space characters need to trimmed so we use Trim() 
            if (!string.IsNullOrEmpty(userProfile.FirstName.Trim()) || !string.IsNullOrEmpty(userProfile.LastName.Trim()))
            {
                userProfile.ID = appUser.ID;
                _proRep.Add(userProfile);
            }
            return View("RegisterOK");

        }

        public ActionResult Activation(Guid id)
        {
            AppUser toBeActivated = _apRep.FirstOrDefault(x => x.ActivationCode == id);
            if (toBeActivated != null)
            {
                toBeActivated.Active = true;
                _apRep.Update(toBeActivated);
                TempData["isAccountActive"] = "Your Account has been activated.";
                return RedirectToAction("Login","Home");
            }

            //in case of a suspicious activity
            TempData["isAccountActive"] = "Account not found.";

            //Todo : need to be logged

            return RedirectToAction("Login", "Home");
            
        }
        public ActionResult RegisterOK()
        { 
            return View(); 
        }
    }
}