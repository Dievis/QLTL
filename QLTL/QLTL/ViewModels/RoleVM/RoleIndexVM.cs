using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.RoleVM
{
    public class RoleIndexVM
    {
        public List<RoleViewModel> Items { get; set; } = new List<RoleViewModel>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; } = 0;
        public string SearchTerm { get; set; }
        public bool? IsDeleted { get; set; }
    }
}