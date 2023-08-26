using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.MVCUI.Controllers
{
    public class ShoppingController : Controller
    {
        // GET: Shopping
        public ActionResult ShoppingList()
        {
            return View();
        }
    }
}