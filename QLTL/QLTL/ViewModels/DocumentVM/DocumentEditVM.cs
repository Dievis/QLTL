using System;
using System.Collections.Generic;

namespace QLTL.ViewModels.DocumentVM
{
    public class DocumentEditVM
    {
        public int DocumentId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int? CategoryId { get; set; }
        public int? DocumentTypeId { get; set; }
        public int UploaderID { get; set; }

        public List<int> DepartmentIds { get; set; } = new List<int>();

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long? FileSize { get; set; }

        public string ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
