using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SareeWeb.DataAccess.Repository;
using SareeWeb.DataAccess.Repository.IRepository;
using SareeWeb.Models;
using SareeWeb.Models.ViewModels;
using SareeWeb.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace SareeApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _UnitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _UnitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _UnitOfWork.ShoppingCart.
                    GetAll(u => u.ApplicationUserId == claims.Value).Count());           
            }
            IEnumerable<Product> ProductList = _UnitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(ProductList);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId=productId,
                Product = _UnitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,CoverType")
            };
            return View(cartObj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity=(ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;
            ShoppingCart cartFromDb = _UnitOfWork.ShoppingCart.GetFirstOrDefault(
                u=>u.ApplicationUserId==claim.Value && u.ProductId==shoppingCart.ProductId);
            if(cartFromDb==null)
            {
                _UnitOfWork.ShoppingCart.Add(shoppingCart);
                _UnitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _UnitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == claim.Value).Count());
            }
            else
            {
                _UnitOfWork.ShoppingCart.Increment(cartFromDb,shoppingCart.Count);
                _UnitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}