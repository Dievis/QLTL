using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.RoleVM
{
    public class RoleViewModel
    {
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "Tên vai trò")]
        public string RoleName { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}