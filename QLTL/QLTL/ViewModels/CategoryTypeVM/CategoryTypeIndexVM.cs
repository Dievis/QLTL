using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.CategoryTypeVM
{
    public class CategoryTypeIndexVM
    {
        public List<CategoryTypeViewModel> Items { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsDeleted { get; set; }
    }
}