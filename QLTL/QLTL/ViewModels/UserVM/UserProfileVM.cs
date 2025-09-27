using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.UserVM
{
    public class UserProfileVM
    {
        public int UserId { get; set; }

        // Cho phép sửa
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string Password { get; set; }        // mật khẩu mới
        public string ConfirmPassword { get; set; } // xác nhận mật khẩu
        public string AvatarUrl { get; set; }       // đường dẫn ảnh (hoặc base64)

        // Không cho sửa
        public string Username { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeCode { get; set; }
        public List<string> RoleNames { get; set; }
    }
}