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
                RemoveImageFromServer(productViewModel.Product.ImageUrl);
                if(file!=null)
                    productViewModel.Product.ImageUrl = AddImageToServer(file);
                
                if(productViewModel.Product.Id==0)
                {
                    _unitOfWork.Product.Add(productViewModel.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productViewModel.Product);
                }

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
                imagePath = Path.Combine("\\"+folderPath , fileName);
            }

            return imagePath;
        }

        private void RemoveImageFromServer(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = Path.Combine(wwwRootPath, fileName.TrimStart('\\'));
                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }
   
        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var products=_unitOfWork.Product.GetAll(includeProperties: "Category");
            return  Json(new { Data=products});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return Json(new { Success = false,message="Product Not Available!" });
            }   
            else
            {
                RemoveImageFromServer(product.ImageUrl);
                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                return Json(new { Success = true, message = "Product Removed Successfully!" });
            }
        }

        #endregion
    }
}
