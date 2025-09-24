using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.RoleVM
{
    // Checkbox cho mỗi Permission
    public class RolePermissionCheckboxVM
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public bool IsAssigned { get; set; } // Có được gán cho Role không
    }

}