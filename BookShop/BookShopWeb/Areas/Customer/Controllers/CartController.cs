using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
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
            return View(ShoppingCartVM);
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
