using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using SareeApp.Data;
using SareeWeb.DataAccess.Repository;
using SareeWeb.Models;

namespace SareeApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CategoryController(IUnitOfWork UnitOfWork)
        {
            _UnitOfWork = UnitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _UnitOfWork.Category.GetAll();
            return View(objCategoryList);
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
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "the dispalyOrder cannot match the name");
            }
            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Add(obj);
                _UnitOfWork.Save();
                TempData["success"] = "Category created successfully";
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
            var categoryDatabase = _UnitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            //var categoryDatabase = _db.Categories.SingleOrDefault(c => c.Id == id);
            //var categoryDatabase=_db.Categories.Find(id);
            if (categoryDatabase == null)
            {
                return NotFound();
            }
            return View(categoryDatabase);
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "the dispalyOrder cannot match the name");
            }
            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Update(obj);
                _UnitOfWork.Save();
                TempData["success"] = "Category updated successfully";
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
            var categoryDatabase = _UnitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            //var categoryDatabase = _db.Categories.SingleOrDefault(c => c.Id == id);
            //var categoryDatabase = _db.Categories.Find(id);
            if (categoryDatabase == null)
            {
                return NotFound();
            }
            return View(categoryDatabase);
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category obj)
        {
            _UnitOfWork.Category.Remove(obj);
            _UnitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
