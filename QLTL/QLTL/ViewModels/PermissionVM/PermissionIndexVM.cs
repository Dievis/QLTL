using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.PermissionVM
{
    public class PermissionIndexVM
    {
        public List<PermissionViewModel> Items { get; set; } = new List<PermissionViewModel>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; } = 0;
        public string SearchTerm { get; set; }
        public bool? IsDeleted { get; set; }
    }
}