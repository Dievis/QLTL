using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.DocumentTypeVM
{
    public class DocumentTypeViewModel
    {
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}