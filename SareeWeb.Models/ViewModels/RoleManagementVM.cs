using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SareeWeb.Models.ViewModels
{
    public class RoleManagementVM
    {
        public IEnumerable<SelectListItem> CompanyList { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
        public ApplicationUser applicationUser { get; set; }
    }
}
