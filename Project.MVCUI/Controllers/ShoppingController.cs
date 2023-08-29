using Microsoft.Ajax.Utilities;
using PagedList;
using Project.BLL.DesignPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using Project.MVCUI.Models.ShoppingTools;
using Project.MVCUI.VMClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Project.MVCUI.Controllers
{
    public class ShoppingController : Controller
    {
        OrderRepository _oRep;
        ProductRepository _pRep;
        CategoryRepository _cRep;
        OrderDetailRepository _odRep;

        public ShoppingController()
        {
            _oRep = new OrderRepository();
            _pRep = new ProductRepository();
            _cRep = new CategoryRepository();
            _odRep = new OrderDetailRepository();
        }
        public ActionResult ShoppingList(int? page, int? categoryID) //nulleable ints will work with the Pagination/PagedList and allow user to select shopping page
        {
            PaginationVM pavm = new PaginationVM
            {
                PagedProducts = categoryID == null ? _pRep.GetActives().ToPagedList(page ?? 1, 9) : _pRep.Where(x => x.CategoryID == categoryID).ToPagedList(page ?? 1, 9),
                Categories = _cRep.GetActives()
            };

            if (categoryID != null) TempData["catID"] = categoryID;
            return View(pavm);
        }
        public ActionResult AddToCart(int id)
        {
            Cart c = Session["scart"] == null ? new Cart() : Session["scart"] as Cart;
            Product addedProduct = _pRep.Find(id);

            CartItem ci = new CartItem()
            {
                ID = addedProduct.ID,
                Name = addedProduct.ProductName,
                Price = addedProduct.UnitPrice,
                ImagePath = addedProduct.ImagePath,
            };
            c.AddCartItem(ci);
            Session["scart"] = c;
            return RedirectToAction("ShoppingList");
        }

        public ActionResult CartPage()
        {
            if (Session["scart"] != null)
            {
                CartPageVM cpvm = new CartPageVM();
                Cart c = Session["scart"] as Cart;
                cpvm.Cart = c;
                return View(cpvm);
            }
            TempData["empty"] = "Your shopping cart is empty";
            return RedirectToAction("ShoppingList");
        }

        public ActionResult DeleteFromCart(int id)
        {
            if (Session["scart"] != null)
            {
                Cart c = Session["scart"] as Cart;
                c.RemoveCartItem(id);
                if (c.MyCart.Count == 0)
                {
                    Session.Remove("scart");
                    TempData["cartEmpty"] = "All items are removed from your cart";
                    return RedirectToAction("ShoppingList");
                }
                return RedirectToAction("CartPage");
            }
            return RedirectToAction("ShoppingList");
        }

        public ActionResult ConfirmOrder()
        {
            AppUser currentUser;

            if (Session["member"] != null)
            {
                currentUser = Session["member"] as AppUser;
            }
            else
            {
                TempData["anonymous"] = "User is not a member!";
            }
            return View();

        }

        //https://localhost:44375/api/Payment/ReceivePayment  this is for the WebApiBank i created to test payment procedure, its on my github page

        [HttpPost]
        public ActionResult ConfirmOrder(OrderVM ovm)
        {
            bool result;
            Cart cart = Session["scart"] as Cart;
            ovm.Order.TotalPrice = ovm.PaymentDTO.ShoppingPrice = cart.TotalPrice;
            #region API section

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44375/api/");
                Task<HttpResponseMessage> postTask = client.PostAsJsonAsync("Payment/ReceivePayment", ovm.PaymentDTO);

                HttpResponseMessage rslt;

                try
                {
                    rslt = postTask.Result;
                }
                catch (Exception ex)
                {
                    TempData["connectionDenied"] = "Your bank denied the connection";
                    return RedirectToAction("ShoppingList");
                }
                if (rslt.IsSuccessStatusCode) result = true;
                else result = false;

                if (result)
                {
                    if (Session["member"] != null)
                    {
                        AppUser user = Session["member"] as AppUser;
                        ovm.Order.AppUserID = user.ID;
                        ovm.Order.UserName = user.UserName;
                    }
                    else
                    {
                        ovm.Order.AppUserID = null;
                        ovm.Order.UserName = TempData["anonymous"].ToString();
                    }

                    _oRep.Add(ovm.Order);// OrderRepository creates Order's ID when triggered

                    foreach (CartItem item in cart.MyCart)
                    {
                        OrderDetail od = new OrderDetail();
                        od.OrderID = ovm.Order.ID;
                        od.ProductID = item.ID;
                        od.TotalPrice = item.SubTotal;
                        od.Quantity = item.Amount;
                        _odRep.Add(od);

                        //at this point we'll drop purchased item's stock from inventory
                        Product reduceStock = _pRep.Find(item.ID);
                        reduceStock.UnitsInStock -= item.Amount;
                        _pRep.Update(reduceStock);
                    }
                    TempData["payment"] = "Your order has been confirmed. Thank you!";
                    MailService.Send(ovm.Order.Email, body: $"Your order has been confirmed. Total Price ->>{ovm.Order.TotalPrice}$");
                    Session.Remove("scart"); //we remove the scart session after order is succesfully confirmed
                    return RedirectToAction("ShoppingList");
                }
                else
                {
                    Task<string> r = rslt.Content.ReadAsStringAsync();
                    TempData["problem"] = r.Result;
                    return RedirectToAction("ShoppingList");
                }
            }

            #endregion
        }
    }
}