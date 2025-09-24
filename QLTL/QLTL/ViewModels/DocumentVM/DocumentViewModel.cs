using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.DocumentVM
{
    public class DocumentViewModel
    {
        public int DocumentId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int? CategoryId { get; set; }
        public int? DocumentTypeId { get; set; }
        public int UploaderID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long? FileSize { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}