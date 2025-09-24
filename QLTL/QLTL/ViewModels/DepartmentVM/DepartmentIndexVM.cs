using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.DepartmentVM
{
    // ViewModel cho phân trang + tìm kiếm
    public class DepartmentIndexVM
    {
        public List<DepartmentViewModel> Items { get; set; } = new List<DepartmentViewModel>();

        // Paging
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        // Search + Filter
        public string SearchTerm { get; set; }
        public bool? IsDeleted { get; set; } // filter trạng thái
    }
}