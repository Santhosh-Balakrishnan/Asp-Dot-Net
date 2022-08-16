using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CoverTypeController : Controller
    {
        IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View(_unitOfWork.CoverType.GetAll());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(coverType);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(coverType);
        }

        public IActionResult Edit(int id)
        {
            var coverType = _unitOfWork.CoverType.GetFirstOrDefault((c) => c.Id == id);
            if (coverType != null)
            {
                return View(coverType);
            }
            TempData["error"] = "Cover Type Not Found";
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(coverType);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(coverType);
        }

        public IActionResult Delete(int id)
        {
            var coverType = _unitOfWork.CoverType.GetFirstOrDefault((c) => c.Id == id);
            if (coverType != null)
            {
                return View(coverType);
            }
            TempData["error"] = "Cover Type Not Found";
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Remove(coverType);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(coverType);
        }
    }
}
