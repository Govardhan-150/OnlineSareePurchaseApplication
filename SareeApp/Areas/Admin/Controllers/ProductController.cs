using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SareeWeb.DataAccess.Repository;
using SareeWeb.Models;
using SareeWeb.Models.ViewModels;

namespace SareeApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork UnitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _UnitOfWork = UnitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ProductViewModel ProductViewModel = new()
            {
                Product = new(),
                CategoryList = _UnitOfWork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                CoverTypeList = _UnitOfWork.CoverType.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
            };

            if (id == null || id == 0)
            {
                //create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(ProductViewModel);
            }
            else
            {
                ProductViewModel.Product = _UnitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(ProductViewModel);
            }
        }
        //when clicked on create button 
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\Product");
                    var extension = Path.GetExtension(file.FileName);
                    if(obj.Product.ImageUrl!=null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using(var FileStreams =new FileStream(Path.Combine(uploads,filename+extension),FileMode.Create))
                    {
                        file.CopyTo(FileStreams);
                    }
                    obj.Product.ImageUrl = @"\Images\Product\" + filename + extension;
                }
                if (obj.Product.Id == 0)
                {
                    _UnitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _UnitOfWork.Product.Update(obj.Product);
                }
                _UnitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var ProductList = _UnitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = ProductList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _UnitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (obj==null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _UnitOfWork.Product.Remove(obj);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Product deleted successfully" });
        }
        #endregion
    }
}
