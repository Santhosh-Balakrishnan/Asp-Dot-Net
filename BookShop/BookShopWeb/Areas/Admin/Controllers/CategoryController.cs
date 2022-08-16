using BookShopWeb.Data;
using BookShop.Models;
using Microsoft.AspNetCore.Mvc;
using BookShop.DataAcess.Repository.IRepository;
using BookShop.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CategoryController : Controller
    {
        private IUnitOfWork unitOfWork;
        public CategoryController(IUnitOfWork _repository)
        {
            unitOfWork = _repository;
        }

        public IActionResult Index()
        {
            return View(unitOfWork.Category.GetAll());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]//Indicates Post method
        [ValidateAntiForgeryToken] //To avoid the cross site forgery attack
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Add(category);//Adds object to the collection
                unitOfWork.Save();//insert the values to the db
                TempData["success"] = "Category added Successfully!";
                return RedirectToAction("Index");//Redirects to action method specified
            }
            return View(category);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = unitOfWork.Product.GetFirstOrDefault((c) => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]//Indicates Post method
        [ValidateAntiForgeryToken] //To avoid the cross site forgery attack
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Update(category);//updates object in the collection
                unitOfWork.Save();//updates the values in the db
                TempData["success"] = "Category updated Successfully!";
                return RedirectToAction("Index");//Redirects to action method specified
            }
            return View(category);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = unitOfWork.Category.GetFirstOrDefault((c) => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]//Indicates Post method
        [ValidateAntiForgeryToken] //To avoid the cross site forgery attack
        public IActionResult Delete(Category category)
        {
            unitOfWork.Category.Remove(category);//Removes object in the collection
            unitOfWork.Save();//Removes the values in the db
            return RedirectToAction("Index");//Redirects to action method specified
        }
    }
}
