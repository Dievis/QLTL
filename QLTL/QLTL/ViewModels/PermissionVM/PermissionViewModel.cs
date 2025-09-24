using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.PermissionVM
{
    public class PermissionViewModel
    {
        public int PermissionId { get; set; }

        [Required]
        [Display(Name = "Tên quyền")]
        public string PermissionName { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}