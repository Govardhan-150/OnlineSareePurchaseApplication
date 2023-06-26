using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SareeApp.Data;
using SareeWeb.DataAccess.Repository;
using SareeWeb.DataAccess.Repository.IRepository;
using SareeWeb.Models;
using SareeWeb.Models.ViewModels;
using SareeWeb.Utility;
using Stripe;
using Stripe.Checkout;
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
			_UnitOfWork = unitOfWork;
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
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult UpdateOrderDetails(OrderVM orderVM)
		{
			var orderHeaderFromDb = _UnitOfWork.OrderHeader.GetFirstOrDefault
				(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
			orderHeaderFromDb.City = orderVM.OrderHeader.City;
			orderHeaderFromDb.State = orderVM.OrderHeader.State;
			orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
			orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
			if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
			{
				orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
			}
			if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
			{
				orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
			}
			_UnitOfWork.OrderHeader.Update(orderHeaderFromDb);
			_UnitOfWork.Save();
			TempData["success"] = "Order details updated sucessfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
		public IActionResult StartProcessing(OrderVM orderVM)
		{
			_UnitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
			_UnitOfWork.Save();
			TempData["success"] = "Order status updated sucessfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
		public IActionResult ShipOrder(OrderVM orderVM)
		{
			var orderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			orderHeader.Carrier = orderVM.OrderHeader.Carrier;
			orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
			orderHeader.OrderStatus = SD.StatusShipped;
			orderHeader.ShippingDate = DateTime.Now;
			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
			}
			_UnitOfWork.OrderHeader.Update(orderHeader);
			_UnitOfWork.Save();
			TempData["success"] = "Order shipped sucessfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
		public IActionResult OrderCancel(OrderVM orderVM)
		{
			var orderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions
				{
					PaymentIntent = orderHeader.PaymentIntentId,
					Reason = RefundReasons.RequestedByCustomer
				};
				var service = new RefundService();
				Refund refund = service.Create(options);
				_UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			_UnitOfWork.OrderHeader.Update(orderHeader);
			_UnitOfWork.Save();
			TempData["success"] = "Order cancelled sucessfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}
		[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayNow(OrderVM orderVM)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			ApplicationUser applicationUser = _UnitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims);
			orderVM.OrderDetail = _UnitOfWork.OrderDetail.GetAll(u => u.OrderId == orderVM.OrderHeader.Id, includeProperties: "Product");
			orderVM.OrderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);
					var domain = "https://localhost:44329/";
					var options = new SessionCreateOptions
					{
						PaymentMethodTypes = new List<string>
					{
						"card",
					},
						LineItems = new List<SessionLineItemOptions>(),
						Mode = "payment",
						SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
						CancelUrl = domain + $"Admin/Order/Details?orderId={orderVM.OrderHeader.Id}",
					};
					foreach (var item in orderVM.OrderDetail)
					{
						var sessionLineItem = new SessionLineItemOptions
						{
							PriceData = new SessionLineItemPriceDataOptions
							{
								UnitAmount = (long)item.Price * 100,//20.00=2000
								Currency = "usd",
								ProductData = new SessionLineItemPriceDataProductDataOptions
								{
									Name = item.Product.ProductName,
								},

							},
							Quantity = item.Count,
						};
						options.LineItems.Add(sessionLineItem);
					}

					var service = new SessionService();
					Session session = service.Create(options);

					_UnitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
					_UnitOfWork.Save();
					Response.Headers.Add("Location", session.Url);
					return new StatusCodeResult(303);
		}
        [ActionName("PaymentConfirmation")]
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _UnitOfWork.OrderHeader.GetFirstOrDefault
                (u => u.Id == orderHeaderId, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //if the user is company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _UnitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _UnitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _UnitOfWork.Save();
                }
            }
            return View(orderHeaderId);

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
