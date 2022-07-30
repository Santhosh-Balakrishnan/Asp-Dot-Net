using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork=unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CompanyDetails(int? id)
        {
            if(id!=null||id > 0)
            {
               var company = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
                if (company == null)
                    return NotFound();
                else return View(company);
            }
            return View(new Company());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompanyDetails(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                    TempData["success"] = "Company added Successfully";
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company updated Successfully";
                }

                _unitOfWork.Save();
              
                return RedirectToAction("Index");
            }
            return View(company);
        }

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _unitOfWork.Company.GetAll();
            return Json(new { Data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var company = _unitOfWork.Company.GetFirstOrDefault(p => p.Id == id);
            if (company == null)
            {
                return Json(new { Success = false, message = "Company Not Available!" });
            }
            else
            {
                _unitOfWork.Company.Remove(company);
                _unitOfWork.Save();
                return Json(new { Success = true, message = "Company Removed Successfully!" });
            }
        }

        #endregion
    }
}
