using System.Collections.Generic;

namespace QLTL.ViewModels.UserVM
{
    public class UserIndexVM
    {
        public List<UserViewModel> Items { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
