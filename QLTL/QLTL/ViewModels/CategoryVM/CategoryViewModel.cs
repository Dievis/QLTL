using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace QLTL.ViewModels.CategoryVM
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [Display(Name = "Loại danh mục")]
        public int CategoryTypeId { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IEnumerable<SelectListItem> CategoryTypeList { get; set; }
        public string CategoryTypeName { get; set; }

    }
}