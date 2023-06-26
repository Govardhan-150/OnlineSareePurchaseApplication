using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeWeb.DataAccess.Repository;
using SareeWeb.Models;
using SareeWeb.Utility;

namespace SareeApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CoverTypeController(IUnitOfWork UnitOfWork)
        {
            _UnitOfWork = UnitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = _UnitOfWork.CoverType.GetAll();
            return View(objCoverTypeList);
        }
        //GET
        public IActionResult Create()
        {
            return View();
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {

            if (ModelState.IsValid)
            {
                _UnitOfWork.CoverType.Add(obj);
                _UnitOfWork.Save();
                TempData["success"] = "CoverType created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var CoverTypeDatabase = _UnitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            //var categoryDatabase = _db.Categories.SingleOrDefault(c => c.Id == id);
            //var categoryDatabase=_db.Categories.Find(id);
            if (CoverTypeDatabase == null)
            {
                return NotFound();
            }
            return View(CoverTypeDatabase);
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.CoverType.Update(obj);
                _UnitOfWork.Save();
                TempData["success"] = "CoverType updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverTypeDatabase = _UnitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            //var categoryDatabase = _db.Categories.SingleOrDefault(c => c.Id == id);
            //var categoryDatabase = _db.Categories.Find(id);
            if (coverTypeDatabase == null)
            {
                return NotFound();
            }
            return View(coverTypeDatabase);
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(CoverType obj)
        {
            _UnitOfWork.CoverType.Remove(obj);
            _UnitOfWork.Save();
            TempData["success"] = "CoverType deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
