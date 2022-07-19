using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ProductDetails(int? id)
        {
            ProductViewModel productViewModel = new ProductViewModel
            {
                Product = new Product(),
                Categories = _unitOfWork.Category.GetAll().Select(c =>
                new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                CoverTypes = _unitOfWork.CoverType.GetAll().Select(c =>
                new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };
            if(id!=null || id > 0)
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    productViewModel.Product = product;
                }
            }
            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductDetails(ProductViewModel productViewModel,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                productViewModel.Product.ImageUrl = AddImageToServer(file);
                _unitOfWork.Product.Add(productViewModel.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product added Successfully";
                return RedirectToAction("Index");
            }
            return View(productViewModel);
        }

        private string AddImageToServer(IFormFile? file)
        {
            string imagePath = "";

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if(file!=null)
            {
                var folderPath = @"images\products";
                var uploadPath = Path.Combine(wwwRootPath, folderPath);
                var extension = Path.GetExtension(file.FileName);
                string fileName = Guid.NewGuid().ToString()+extension;
                var imageFile = Path.Combine(uploadPath, fileName);
                using(var fileStream=new FileStream(imageFile,FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                imagePath = Path.Combine(folderPath , fileName);
            }

            return imagePath;
        }

        public IActionResult Edit(int id)
        {
            TempData["error"] = "Cover Type Not Found";
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public IActionResult Delete(int id)
        {
            var coverType = _unitOfWork.CoverType.GetFirstOrDefault(p => p.Id == id);
            if (coverType != null)
            {
                return View(coverType);
            }
            TempData["error"] = "Cover Type Not Found";
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var products=_unitOfWork.Product.GetAll(includeProperties: "Category");
            return  Json(new { Data=products});
        }

        #endregion
    }
}
