using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SareeWeb.DataAccess.Repository;
using SareeWeb.Models;
using SareeWeb.Models.ViewModels;
using SareeWeb.Utility;

namespace SareeApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CompanyController(IUnitOfWork UnitOfWork)
        {
            _UnitOfWork = UnitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Company company = new();

            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _UnitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _UnitOfWork.Company.Add(obj);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    _UnitOfWork.Company.Update(obj);
                    TempData["success"] = "Company updated successfully";
                }
                _UnitOfWork.Save();
                
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _UnitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _UnitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
            if (obj==null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _UnitOfWork.Company.Remove(obj);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Company deleted successfully" });
        }
        #endregion
    }
}
