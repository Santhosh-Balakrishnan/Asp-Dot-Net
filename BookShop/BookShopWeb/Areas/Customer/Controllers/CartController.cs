using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using BookShop.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        //Adding Comment in Cart Controller
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var cartItems=_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,includeProperties:"Product");
            ShoppingCartVM = new ShoppingCartViewModel
            {
                Items = cartItems,
                OrderHeader=new OrderHeader()
            };
            UpdateCartItemPrice(ShoppingCartVM);
            return View(ShoppingCartVM);
        }

        public IActionResult Increment(int cartId)
        {
            var shoppingCartItem = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
            if(shoppingCartItem!=null)
            {
                _unitOfWork.ShoppingCart.IncrementCount(shoppingCartItem, 1);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Decrement(int cartId)
        {
            var shoppingCartItem = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Count <= 1)
                {
                    _unitOfWork.ShoppingCart.Remove(shoppingCartItem);
                }
                else
                {
                    _unitOfWork.ShoppingCart.DecrementCount(shoppingCartItem, 1);
                }
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var shoppingCartItem = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
            if (shoppingCartItem != null)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCartItem);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var cartItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
            ShoppingCartVM = new ShoppingCartViewModel
            {
                Items = cartItems,
                OrderHeader = new OrderHeader()
            };
            UpdateOrderHeader(claim, ShoppingCartVM.OrderHeader);
            UpdateCartItemPrice(ShoppingCartVM);
            return Payment(ShoppingCartVM);
        }

        private IActionResult Payment(ShoppingCartViewModel shoppingCartVM)
        {
            var domain = "https://localhost:44372/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
            };
            
            foreach (var item in ShoppingCartVM.Items)
            {
                var sessionItem = new SessionLineItemOptions
                {
                    PriceData=new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price,
                        Currency = "inr",
                        ProductData=new SessionLineItemPriceDataProductDataOptions
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
            _unitOfWork.OrderHeaderRepository.UpdatePaymentStatus(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        //public IActionResult OrderConfirmation(int id)
        //{
        //    OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == id);
        //    var service = new SessionService();
        //    Session session = service.Get(orderHeader.SessionId);

        //    if(session.PaymentStatus.ToLower()=="paid")
        //    {
        //        _unitOfWork.OrderHeaderRepository.UpdateOrderStatus(id, OrderStatus.Approved.ToString(), PaymentStatus.Approved.ToString());
        //        _unitOfWork.Save();
        //    }
        //}

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult OrderSummary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.Items = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.Pending.ToString();
            ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.Pending.ToString();
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
             UpdateCartItemPrice(ShoppingCartVM);
            _unitOfWork.OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.Items)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.Items);
            return RedirectToAction("Index","Home");
        }
        private void UpdateOrderHeader(Claim claim,OrderHeader orderHeader)
        {
            orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            orderHeader.Name = orderHeader.ApplicationUser.Name;
            orderHeader.PhoneNumber = orderHeader.ApplicationUser.PhoneNumber;
            orderHeader.StreetAddress = orderHeader.ApplicationUser.Address;
            orderHeader.City = orderHeader.ApplicationUser.City;
            orderHeader.State = orderHeader.ApplicationUser.State;
            orderHeader.PostalCode = orderHeader.ApplicationUser.PostalCode;
        }

        private void UpdateCartItemPrice(ShoppingCartViewModel ShoppingCartVM)
        {

            foreach (var item in ShoppingCartVM.Items)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                ShoppingCartVM.OrderHeader.OrderTotal += item.Price * item.Count;
            }
        }
        private double GetPriceBasedOnQuantity(ShoppingCart cartItem)
        {
            if (cartItem.Count >= 100)
                return cartItem.Product.Price100;
            else if (cartItem.Count >= 50)
                return cartItem.Product.Price50;
            else return cartItem.Product.Price;
        }
    }
}
