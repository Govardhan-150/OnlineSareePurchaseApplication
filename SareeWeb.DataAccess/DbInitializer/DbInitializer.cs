using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SareeApp.Data;
using SareeWeb.Models;
using SareeWeb.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SareeWeb.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DbInitializer(UserManager<IdentityUser> UserManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _UserManager = UserManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            //create the migrations, if any migrations are left
            try
            {
                if(_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { }

            //roles are created, if no roles are created

            if(!(_roleManager.RoleExistsAsync(SD.Role_User_Indi).GetAwaiter().GetResult()))
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();

                //roles are created along with admin user

                _UserManager.CreateAsync(new ApplicationUser
                {
                    UserName = "govaAdmin@gmail.com",
                    Email = "govaAdmin@gmail.com",
                    City = "Hindupur",
                    State = "Ap",
                    StreetAddress = "23#,MrPalli",
                    PhoneNumber = "7036126399",
                    PostalCode = "567899",
                    Name = "Govardhan"

                }, "Admin@150").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "govaAdmin@gmail.com");
                _UserManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
