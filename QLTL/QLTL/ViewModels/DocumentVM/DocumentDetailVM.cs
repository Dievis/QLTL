using QLTL.Models;
using QLTL.ViewModels.DepartmentVM; // import namespace chứa DepartmentViewModel
using System;
using System.Collections.Generic;
using System.Linq;

namespace QLTL.ViewModels.DocumentVM
{
    public class DocumentDetailVM
    {
        public int DocumentId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }

        public int? DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        public int UploaderID { get; set; }
        public string UploaderName { get; set; }

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        public List<int> DepartmentIds => Departments?.Select(d => d.DepartmentId).ToList() ?? new List<int>();

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long? FileSize { get; set; }

        public int? ApprovalId { get; set; }   // thêm dòng này

        public string ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
