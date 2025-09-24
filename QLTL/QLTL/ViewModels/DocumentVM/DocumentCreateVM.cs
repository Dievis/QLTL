namespace QLTL.ViewModels.DocumentVM
{
    public class DocumentCreateVM
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public int? CategoryId { get; set; }
        public int? DocumentTypeId { get; set; }
        public int UploaderID { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long? FileSize { get; set; }
    }
}
