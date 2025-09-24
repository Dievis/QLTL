using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace QLTL.ViewModels.UserVM
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }

        // Role
        public string RoleName { get; set; }
        public int? RoleId { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }  // ✅ để binding DropDownListFor

        // Department
        public string DepartmentName { get; set; }
        public int? DepartmentId { get; set; }

        // Password (chỉ dùng khi Create)
        public string Password { get; set; }

        // Flags
        public bool IsSuperAdmin { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<int> SelectedRoleIds { get; set; } = new List<int>();
        public List<string> RoleNames { get; set; } = new List<string>();


    }
}
