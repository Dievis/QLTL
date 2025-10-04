using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.UserVM
{
    public class UserEditVM
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một vai trò")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Vui lòng chọn phòng ban")]
        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; }
    }

}