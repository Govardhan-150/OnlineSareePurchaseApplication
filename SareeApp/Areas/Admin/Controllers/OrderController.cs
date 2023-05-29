using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeApp.Data;
using SareeWeb.DataAccess.Repository;
using SareeWeb.DataAccess.Repository.IRepository;
using SareeWeb.Models;
using SareeWeb.Models.ViewModels;
using SareeWeb.Utility;
using System.Security.Claims;

namespace SareeApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _UnitOfWork;
		[BindProperty]
		public OrderVM orderVM { get; set; }
		public OrderController(IUnitOfWork unitOfWork)
		{
			_UnitOfWork= unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int? orderId)
        {
			orderVM = new OrderVM()
			{
				OrderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _UnitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
			};
            return View(orderVM);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;
			if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
				orderHeaders = _UnitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
			}
			else
			{
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var Claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
				orderHeaders = _UnitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId==Claims.Value,includeProperties: "ApplicationUser");
            }
				switch(status)
			{
				case "pending":
					orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
					break;
				case "approved":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				case "inprocess":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
				case "completed":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
				default:
					break;

			}
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
