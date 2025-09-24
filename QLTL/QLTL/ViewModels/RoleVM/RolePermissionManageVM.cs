using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.RoleVM
{
    // ViewModel tổng hợp để quản lý permissions
    public class RolePermissionManageVM
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<RolePermissionCheckboxVM> Permissions { get; set; } = new List<RolePermissionCheckboxVM>();
    }
}