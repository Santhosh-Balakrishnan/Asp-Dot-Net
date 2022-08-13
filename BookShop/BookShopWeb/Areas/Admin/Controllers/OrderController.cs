using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using BookShop.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
