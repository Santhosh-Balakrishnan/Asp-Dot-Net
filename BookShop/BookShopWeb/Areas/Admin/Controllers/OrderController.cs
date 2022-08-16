using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using BookShop.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderViewModel OrderViewModel { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderViewModel = new OrderViewModel
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderId == orderId, includeProperties: "Product")
            };
            return View(OrderViewModel);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayNow()
        {
            OrderViewModel.OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == OrderViewModel.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderViewModel.OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderId == OrderViewModel.OrderHeader.Id, includeProperties: "Product");
            return Payment(OrderViewModel);
        }

        private IActionResult Payment(OrderViewModel orderViewModel)
        {
            var domain = "https://localhost:44372/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderId={orderViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderViewModel.OrderHeader.Id}",
            };

            foreach (var item in orderViewModel.OrderDetails)
            {
                var sessionItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price * 100,
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeaderRepository.UpdatePaymentStatus(orderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == orderId);
            if (orderHeader.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment.ToString())
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderId, orderHeader.OrderStatus, PaymentStatus.Approved.ToString());
                    _unitOfWork.Save();
                }
            }
            return View(orderId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == OrderViewModel.OrderHeader.Id,tracked:false);
            orderHeader.Name = OrderViewModel.OrderHeader.Name;
            orderHeader.PhoneNumber = OrderViewModel.OrderHeader.PhoneNumber;
            orderHeader.StreetAddress = OrderViewModel.OrderHeader.StreetAddress;
            orderHeader.City = OrderViewModel.OrderHeader.City;
            orderHeader.State = OrderViewModel.OrderHeader.State;
            orderHeader.PostalCode = OrderViewModel.OrderHeader.PostalCode;
            if(OrderViewModel.OrderHeader.Carrier!=null)
            {
                orderHeader.Carrier = OrderViewModel.OrderHeader.Carrier;
            }
            if (OrderViewModel.OrderHeader.TrackingNumber != null)
            {
                orderHeader.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeaderRepository.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
        public IActionResult StartProcessing()
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == OrderViewModel.OrderHeader.Id, tracked: false);
           
            _unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderHeader.Id,OrderStatus.Processing.ToString());
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == OrderViewModel.OrderHeader.Id, tracked: false);
            orderHeader.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = OrderStatus.Shipped.ToString();
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus==PaymentStatus.ApprovedForDelayedPayment.ToString())
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeaderRepository.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == OrderViewModel.OrderHeader.Id, tracked: false);
            if(orderHeader.PaymentStatus==PaymentStatus.Approved.ToString())
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                var refund = service.Create(options);
                _unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderHeader.Id, OrderStatus.Cancelled.ToString(), PaymentStatus.Refunded.ToString());
            }
            else {
                _unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderHeader.Id, OrderStatus.Cancelled.ToString());
            }
            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeader.Id });
        }
        #region API Calls

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            var users = _unitOfWork.ApplicationUser.GetAll();
            IEnumerable<OrderHeader> orders;
            if(User.IsInRole(Role.Admin.ToString())|| User.IsInRole(Role.Employee.ToString()))
            {
                orders = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claim = (ClaimsIdentity)User.Identity;
                var user = claim.FindFirst(ClaimTypes.NameIdentifier);
                var userId = user.Value;
                orders = _unitOfWork.OrderHeaderRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }
         
            if(!(string.IsNullOrEmpty(status) || status=="all"))
                orders = orders.Where(o=>o.OrderStatus==status);
            
            return Json(new { Data = orders });
        }


        #endregion

    }
}
