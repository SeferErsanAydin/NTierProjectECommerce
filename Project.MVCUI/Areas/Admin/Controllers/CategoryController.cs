using Project.BLL.DesignPatterns.GenericRepository.ConcRep;
using Project.MVCUI.Areas.Admin.AdminVMClasses;
using Project.MVCUI.AuthenticationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.MVCUI.Areas.Admin.Controllers
{
    //[AdminAuthentication]
    public class CategoryController : Controller
    {

        CategoryRepository _cRep;
        public CategoryController()
        {
            _cRep = new CategoryRepository();
        }
        public ActionResult CategoryList(int? id)
        {
            CategoryVM cvm = id == null ? new CategoryVM()
            {
                Categories = _cRep.GetActives()
            } : new CategoryVM { Categories = _cRep.Where(x => x.ID == id) };        
            return View(cvm);
        }
    }
}