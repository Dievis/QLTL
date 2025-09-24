using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.DocumentVM
{
    public class DocumentIndexVM
    {
        public List<DocumentDetailVM> Items { get; set; } // đổi kiểu từ DocumentViewModel sang DocumentDetailVM
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsDeleted { get; set; }
    }

}