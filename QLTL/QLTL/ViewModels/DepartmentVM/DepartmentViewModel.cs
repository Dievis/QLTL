using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.DepartmentVM
{
    // ViewModel chính cho Department
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [Display(Name = "Tên phòng ban")]
        public string DepartmentName { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}