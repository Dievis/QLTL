using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.CategoryTypeVM
{
    public class CategoryTypeViewModel
    {
        public int CategoryTypeId { get; set; }

        [Required]
        [Display(Name = "Tên loại danh mục")]
        public string CategoryTypeName { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public bool IsDeleted { get; set; }          // Không cần nullable
        public DateTime CreatedAt { get; set; }      // Luôn có giá trị
        public DateTime? UpdatedAt { get; set; }     // Có thể null
    }
}