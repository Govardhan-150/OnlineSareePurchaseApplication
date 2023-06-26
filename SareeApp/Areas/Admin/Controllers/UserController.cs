using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using SareeApp.Data;
using SareeWeb.DataAccess.Repository;
using SareeWeb.Models;
using SareeWeb.Models.ViewModels;
using SareeWeb.Utility;

namespace SareeApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(IUnitOfWork unitOfWork,
                               RoleManager<IdentityRole> roleManager,
                               UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }



        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

            //var userRoles = _roleManager.UserRoles.ToList();//RoleId,UserId
            //var Roles = _db.Roles.ToList();
            foreach (var user in objUserList)
            {
                //var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                //var Role = Roles.FirstOrDefault(u => u.Id == roleId).Name;
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = objUserList });
        }
        #endregion

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {

            var objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.ApplicationUser.update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        [HttpGet]
        [ActionName("RoleManagement")]
        public IActionResult RoleManagement(string userId)
        {
            //var userRoles = _unitOfWork.ApplicationUser.UserRoles.ToList();//RoleId,UserId
            //var Roles=_roleManager.Roles.ToList();
            //var roleId = userRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagementVM rolemanagement = new()
            {
                applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u=>u.Id==userId,includeProperties: "Company"),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                })
            };
            rolemanagement.applicationUser.Role = _userManager.GetRolesAsync
                (_unitOfWork.ApplicationUser.GetFirstOrDefault(u=>u.Id==userId)).
                GetAwaiter().GetResult().FirstOrDefault();

            return View(rolemanagement);
        }
        [HttpPost,ActionName("RoleManagement")]
        public IActionResult RoleManagementPost(RoleManagementVM roleManagementVM)
        {
            //to get the role name from the database
            //string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.applicationUser.Id).RoleId;
            string oldrole = _userManager.GetRolesAsync
                (_unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == roleManagementVM.applicationUser.Id)).
                GetAwaiter().GetResult().FirstOrDefault();
            ApplicationUser applicationUserFromdb = _unitOfWork.ApplicationUser.GetFirstOrDefault
                (u => u.Id == roleManagementVM.applicationUser.Id);

            if (!(roleManagementVM.applicationUser.Role==oldrole))
            {
                //role is updated
                if(roleManagementVM.applicationUser.Role==SD.Role_User_Comp)
                {
                    applicationUserFromdb.CompanyId=roleManagementVM.applicationUser.CompanyId;
                }
                if(oldrole==SD.Role_User_Comp)
                {
                    applicationUserFromdb.CompanyId = null;
                }

                _unitOfWork.ApplicationUser.update(applicationUserFromdb);
                _unitOfWork.Save();
                _userManager.RemoveFromRoleAsync(applicationUserFromdb, oldrole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUserFromdb, roleManagementVM.applicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldrole==SD.Role_User_Comp && applicationUserFromdb.CompanyId!=roleManagementVM.applicationUser.CompanyId)
                {
                    applicationUserFromdb.CompanyId = roleManagementVM.applicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.update(applicationUserFromdb);
                    _unitOfWork.Save();
                }
            }
            return RedirectToAction("Index", "User");
        }

    }
}
