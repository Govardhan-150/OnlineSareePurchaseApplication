using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using SareeWeb.DataAccess.Repository;
using SareeWeb.Utility;
using System.Security.Claims;

namespace SareeApp.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.
                    GetAll(u => u.ApplicationUserId == claims.Value).Count());
                return View(HttpContext.Session.GetString(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
