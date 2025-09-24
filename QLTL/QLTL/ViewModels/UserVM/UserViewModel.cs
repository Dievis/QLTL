using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace QLTL.ViewModels.UserVM
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }

        // Role
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một vai trò")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();   // lưu danh sách role đã chọn khi Create/Edit

        public IEnumerable<SelectListItem> RoleOptions { get; set; }        // để binding DropDownListFor/CheckboxListFor

        public List<string> RoleNames { get; set; } = new List<string>();   // hiển thị danh sách role trong Index/Details

        // Department
        public string DepartmentName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phòng ban")]
        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        // Flags
        public bool IsSuperAdmin { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}
