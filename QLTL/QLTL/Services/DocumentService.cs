using LinqKit;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.DepartmentVM;
using QLTL.ViewModels.DocumentVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mime; // cho ContentDisposition


namespace QLTL.Services
{
    public class DocumentService
    {
        private readonly IGenericRepository<Document> _repo;
        private readonly IGenericRepository<FavoriteDocument> _favRepo;
        private readonly IGenericRepository<DocumentDepartment> _docDeptRepo;
        private readonly IGenericRepository<DocumentApproval> _approvalRepo;
        private readonly IGenericRepository<DocumentChangeLog> _changeLogRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<DocumentType> _docTypeRepo;
        private readonly IGenericRepository<Department> _departmentRepo;


        private readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        //private readonly string UploadFolder = "/Uploads/Documents";

        public DocumentService(
            IGenericRepository<Document> repo,
            IGenericRepository<FavoriteDocument> favRepo,
            IGenericRepository<DocumentDepartment> docDeptRepo,
            IGenericRepository<DocumentApproval> approvalRepo,
            IGenericRepository<DocumentChangeLog> changeLogRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<Category> categoryRepo,
            IGenericRepository<DocumentType> docTypeRepo,
            IGenericRepository<Department> departmentRepo)
        {
            _repo = repo;
            _favRepo = favRepo;
            _docDeptRepo = docDeptRepo;
            _approvalRepo = approvalRepo;
            _changeLogRepo = changeLogRepo;
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
            _docTypeRepo = docTypeRepo;
            _departmentRepo = departmentRepo;
        }

        // ================== LẤY DANH SÁCH CÓ PHÂN TRANG ==================
        public async Task<DocumentIndexVM> GetAllAsync(string search, bool? isDeleted, int pageIndex, int pageSize)
        {
            // Tạo filter động
            var filter = PredicateBuilder.New<Document>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(d => d.Title.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(d => d.IsDeleted == isDeleted.Value);

            // Lấy dữ liệu phân trang từ repository
            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(d => d.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            // Map sang ViewModel
            var itemVMs = new List<DocumentDetailVM>();
            foreach (var d in items)
                itemVMs.Add(await MapToDetailVM(d));

            return new DocumentIndexVM
            {
                Items = itemVMs,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }

        // ================== LẤY DANH SÁCH TÀI LIỆU ĐÃ DUYỆT CÓ PHÂN TRANG ==================
        public async Task<DocumentIndexVM> GetApprovedDocumentsAsync(string search, int pageIndex, int pageSize)
        {
            // Tạo filter động
            var filter = PredicateBuilder.New<Document>(true);

            // Chỉ lấy tài liệu đã duyệt
            filter = filter.And(d => d.ApprovalStatus == "Approved");

            // Không lấy tài liệu đã xóa mềm
            filter = filter.And(d => d.IsDeleted == false || d.IsDeleted == null);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(d => d.Title.Contains(search) || d.Content.Contains(search));

            // Lấy dữ liệu phân trang từ repository
            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(d => d.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            // Map sang ViewModel (giống GetAllAsync)
            var itemVMs = new List<DocumentDetailVM>();
            foreach (var d in items)
                itemVMs.Add(await MapToDetailVM(d));

            return new DocumentIndexVM
            {
                Items = itemVMs,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = false
            };
        }



        // ================== GET BY ID ==================
        public async Task<DocumentDetailVM> GetByIdAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return null;

            return await MapToDetailVM(d);
        }

        // ================== CREATE ==================
        public async Task<string> CreateAsync(DocumentCreateVM model, HttpPostedFileBase file, string serverRootPath, List<int> departmentIds = null)
        {
            if (file == null || file.ContentLength == 0)
                return "Vui lòng chọn file.";

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                return "Định dạng file không được hỗ trợ.";

            if (file.ContentLength > MaxFileSize)
                return $"Dung lượng file tối đa là {MaxFileSize / (1024 * 1024)}MB.";

            try
            {
                // Xử lý đường dẫn upload
                string uploadsFolderRelative = "Uploads/Documents"; // không dùng ~
                string uploadsPhysicalFolder = Path.Combine(serverRootPath, uploadsFolderRelative);

                // Tạo folder nếu chưa tồn tại
                if (!Directory.Exists(uploadsPhysicalFolder))
                    Directory.CreateDirectory(uploadsPhysicalFolder);

                if (!Directory.Exists(uploadsPhysicalFolder))
                    return $"Không thể tạo folder lưu trữ: {uploadsPhysicalFolder}";

                // Tạo tên file duy nhất
                string savedFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                string savedPath = Path.Combine(uploadsPhysicalFolder, savedFileName);

                // Lưu file
                file.SaveAs(savedPath);

                if (!File.Exists(savedPath))
                    return $"File chưa được lưu: {savedPath}";

                // Tạo entity Document
                var entity = new Document
                {
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    DocumentTypeId = model.DocumentTypeId,
                    UploaderID = model.UploaderID,
                    FileName = savedFileName,
                    FilePath = "/" + uploadsFolderRelative.Replace("\\", "/") + "/" + savedFileName,
                    FileType = ext,
                    FileSize = file.ContentLength,
                    ApprovalStatus = "Pending",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _repo.AddAsync(entity);
                await _repo.SaveChangesAsync();

                if (departmentIds != null && departmentIds.Any())
                    await AssignDepartmentsAsync(entity.DocumentId, departmentIds);

                var approval = new DocumentApproval
                {
                    DocumentID = entity.DocumentId,
                    UploaderID = entity.UploaderID,
                    Status = "Pending",
                    DateUploaded = DateTime.Now
                };
                await _approvalRepo.AddAsync(approval);
                await _approvalRepo.SaveChangesAsync();

                await LogChangeAsync(entity.DocumentId, entity.UploaderID, "Create", $"Tạo tài liệu {entity.Title}");

                return null;
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                return $"Lỗi khi lưu file: {ex.Message}";
            }
        }


        // ================== UPDATE ==================
        public async Task<string> UpdateAsync(DocumentEditVM model, HttpPostedFileBase file, string serverRootPath, List<int> departmentIds = null)
        {
            var entity = await _repo.GetByIdAsync(model.DocumentId);
            if (entity == null) return "Không tìm thấy tài liệu.";

            var oldFileName = entity.FileName;

            entity.Title = model.Title;
            entity.Content = model.Content;
            entity.CategoryId = model.CategoryId;
            entity.DocumentTypeId = model.DocumentTypeId;
            entity.UpdatedAt = DateTime.Now;

            if (file != null && file.ContentLength > 0)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!AllowedExtensions.Contains(ext))
                    return "Định dạng file không được hỗ trợ.";

                if (file.ContentLength > MaxFileSize)
                    return $"Dung lượng file tối đa là {MaxFileSize / (1024 * 1024)}MB.";

                try
                {
                    string uploadsFolderRelative = "Uploads/Documents";
                    string uploadsPhysicalFolder = Path.Combine(serverRootPath, uploadsFolderRelative);

                    if (!Directory.Exists(uploadsPhysicalFolder))
                        Directory.CreateDirectory(uploadsPhysicalFolder);

                    if (!Directory.Exists(uploadsPhysicalFolder))
                        return $"Không thể tạo folder lưu trữ: {uploadsPhysicalFolder}";

                    string savedFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                    string savedPath = Path.Combine(uploadsPhysicalFolder, savedFileName);

                    file.SaveAs(savedPath);

                    if (!File.Exists(savedPath))
                        return $"File chưa được lưu: {savedPath}";

                    // Cập nhật thông tin file
                    entity.FileName = savedFileName;
                    entity.FilePath = "/" + uploadsFolderRelative.Replace("\\", "/") + "/" + savedFileName;
                    entity.FileType = ext;
                    entity.FileSize = file.ContentLength;

                    // Xóa file cũ nếu có
                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        var oldPhysical = Path.Combine(uploadsPhysicalFolder, oldFileName);
                        if (File.Exists(oldPhysical)) File.Delete(oldPhysical);
                    }
                }
                catch (Exception ex)
                {
                    return $"Lỗi khi lưu file mới: {ex.Message}";
                }
            }

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            if (departmentIds != null && departmentIds.Any())
                await AssignDepartmentsAsync(entity.DocumentId, departmentIds);

            await LogChangeAsync(entity.DocumentId, model.UploaderID, "Update",
                $"Cập nhật tài liệu {entity.Title} (file cũ: {oldFileName}, mới: {entity.FileName})");

            return null;
        }


        // ================== LẤY TẤT CẢ DANH MỤC ==================
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepo.GetAllAsync(c => c.IsDeleted == false);
        }

        // ================== LẤY TẤT CẢ LOẠI TÀI LIỆU ==================
        public async Task<IEnumerable<DocumentType>> GetAllDocumentTypesAsync()
        {
            return await _docTypeRepo.GetAllAsync();
        }

        // ================== LẤY TẤT CẢ PHÒNG BAN ==================
        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _departmentRepo.GetAllAsync(d => d.IsDeleted == false);
        }

        // ================== LẤY TÀI LIỆU THEO PHÒNG BAN ==================
        public async Task<IEnumerable<Department>> GetDepartmentsByDocumentAsync(int documentId)
        {
            var links = (await _docDeptRepo.GetAllAsync(dd => dd.DocumentId == documentId && dd.IsDeleted == false)).ToList();
            var deps = new List<Department>();
            foreach (var link in links)
            {
                var dep = await _departmentRepo.GetByIdAsync(link.DepartmentId);
                if (dep != null) deps.Add(dep);
            }
            return deps;
        }
        // ================== XÓA MỀM ==================
        public async Task SoftDeleteDocumentAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        // ================== ASSIGN DEPARTMENT ==================
        private async Task AssignDepartmentsAsync(int documentId, List<int> departmentIds)
        {
            var current = (await _docDeptRepo.GetAllAsync(dd => dd.DocumentId == documentId)).ToList();

            foreach (var dd in current)
            {
                if (!departmentIds.Contains(dd.DepartmentId))
                {
                    dd.IsDeleted = true;
                    dd.DeletedAt = DateTime.Now;
                    await _docDeptRepo.UpdateAsync(dd);
                }
            }

            foreach (var depId in departmentIds.Distinct())
            {
                if (!current.Any(x => x.DepartmentId == depId && x.IsDeleted == false))
                {
                    var newLink = new DocumentDepartment
                    {
                        DocumentId = documentId,
                        DepartmentId = depId,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now
                    };
                    await _docDeptRepo.AddAsync(newLink);
                }
            }

            await _docDeptRepo.SaveChangesAsync();
        }

        // ================== FAVORITE ==================
        // Danh sách yêu thích có phân trang
        public async Task<DocumentIndexVM> GetFavoritesAsync(int userId, string search, int pageIndex, int pageSize)
        {
            // Lấy tất cả FavoriteDocument của user
            var favs = (await _favRepo.GetAllAsync(f => f.UserId == userId)).ToList();

            // Lọc các document chưa xóa
            var docs = new List<Document>();
            foreach (var fav in favs)
            {
                var doc = await _repo.GetByIdAsync(fav.DocumentId);
                if (doc != null && doc.IsDeleted != true)
                {
                    if (string.IsNullOrEmpty(search) || doc.Title.Contains(search))
                        docs.Add(doc);
                }
            }

            // Tổng số bản ghi
            var total = docs.Count;

            // Phân trang
            var pagedDocs = docs
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map sang ViewModel
            var itemVMs = new List<DocumentDetailVM>();
            foreach (var d in pagedDocs)
            {
                itemVMs.Add(await MapToDetailVM(d));
            }

            return new DocumentIndexVM
            {
                Items = itemVMs,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = false
            };
        }

        // Kiểm tra xem người dùng đã đánh dấu yêu thích hay chưa
        public async Task<bool> IsFavoriteAsync(int documentId, int userId)
        {
            return (await _favRepo.GetAllAsync(f => f.DocumentId == documentId && f.UserId == userId)).Any();
        }

        // Thêm yêu thích
        public async Task<string> AddFavoriteAsync(int documentId, int userId)
        {
            var exists = (await _favRepo.GetAllAsync(f => f.DocumentId == documentId && f.UserId == userId)).Any();
            if (exists)
                return "Tài liệu đã được đánh dấu yêu thích.";

            var fav = new FavoriteDocument
            {
                DocumentId = documentId,
                UserId = userId
            };

            await _favRepo.AddAsync(fav);
            await _favRepo.SaveChangesAsync();
            return null;
        }

        // Xóa yêu thích (delete luôn)
        public async Task<string> RemoveFavoriteAsync(int documentId, int userId)
        {
            var favs = (await _favRepo.GetAllAsync(f => f.DocumentId == documentId && f.UserId == userId)).ToList();
            if (!favs.Any())
                return "Tài liệu chưa được đánh dấu yêu thích.";

            foreach (var fav in favs)
            {
                await _favRepo.DeleteAsync(fav); // xóa bản ghi thật
            }

            await _favRepo.SaveChangesAsync();
            return null;
        }




        // ================== LOG ==================
        private async Task LogChangeAsync(int documentId, int userId, string changeType, string description = null)
        {
            var userExists = (await _userRepo.GetAllAsync(u => u.UserId == userId)).Any();
            if (!userExists)
                throw new Exception($"UserId {userId} không tồn tại, không thể ghi log");

            var log = new DocumentChangeLog
            {
                DocumentID = documentId,
                ChangedBy = userId,
                ChangeType = changeType,
                ChangeDescription = description,
                ChangeDate = DateTime.Now
            };

            await _changeLogRepo.AddAsync(log);
            await _changeLogRepo.SaveChangesAsync();
        }

        // ================== APPROVAL ==================
        public async Task CreateApprovalRequestAsync(int documentId, int uploaderId, string reason = null)
        {
            var approval = new DocumentApproval
            {
                DocumentID = documentId,
                UploaderID = uploaderId,
                ApproverID = null,
                Status = "Pending",
                Reason = reason,
                DateUploaded = DateTime.Now
            };

            await _approvalRepo.AddAsync(approval);
            await _approvalRepo.SaveChangesAsync();

            var doc = await _repo.GetByIdAsync(documentId);
            if (doc != null)
            {
                doc.ApprovalStatus = "Pending";
                await _repo.UpdateAsync(doc);
                await _repo.SaveChangesAsync();
            }
        }

        public async Task<string> ReviewApprovalAsync(int approvalId, int approverId, string newStatus, string reason = null)
        {
            // Kiểm tra trạng thái hợp lệ
            var validStatuses = new[] { "Approved", "Rejected" };
            if (!validStatuses.Contains(newStatus))
                return "Trạng thái duyệt không hợp lệ.";

            var approval = await _approvalRepo.GetByIdAsync(approvalId);
            if (approval == null)
                return "Yêu cầu phê duyệt không tồn tại.";

            // Nếu đã duyệt rồi thì không cho duyệt lại
            if (approval.Status != "Pending")
                return $"Yêu cầu đã được xử lý với trạng thái {approval.Status}.";

            // Cập nhật thông tin duyệt
            approval.ApproverID = approverId;
            approval.Status = newStatus;
            approval.Reason = reason;
            approval.DateReviewed = DateTime.Now;
            await _approvalRepo.UpdateAsync(approval);

            // Cập nhật Document liên quan
            var doc = await _repo.GetByIdAsync(approval.DocumentID);
            if (doc != null)
            {
                doc.ApprovalStatus = newStatus;
                doc.ApprovalDate = DateTime.Now;
                await _repo.UpdateAsync(doc);
            }

            // Lưu cả hai thay đổi cùng lúc (transaction-like)
            await _approvalRepo.SaveChangesAsync();
            await _repo.SaveChangesAsync();

            // Ghi log duyệt
            await LogChangeAsync(
                approval.DocumentID,
                approverId,
                "Approval",
                $"Phê duyệt tài liệu: {newStatus}. Lý do: {reason}"
            );

            return null;
        }

        public async Task<string> CancelApprovalAsync(int approvalId, int uploaderId)
        {
            var approval = await _approvalRepo.GetByIdAsync(approvalId);
            if (approval == null)
                return "Yêu cầu phê duyệt không tồn tại.";

            // Chỉ người tạo tài liệu mới được hủy duyệt
            if (approval.UploaderID != uploaderId)
                return "Bạn không có quyền hủy phê duyệt này.";

            // Reset trạng thái về Pending
            approval.Status = "Pending";
            approval.Reason = null;
            approval.DateReviewed = null;
            approval.ApproverID = null; // clear người duyệt
            await _approvalRepo.UpdateAsync(approval);

            // Cập nhật document liên quan
            var doc = await _repo.GetByIdAsync(approval.DocumentID);
            if (doc != null)
            {
                doc.ApprovalStatus = "Pending";
                doc.ApprovalDate = null;
                await _repo.UpdateAsync(doc);
            }

            await _approvalRepo.SaveChangesAsync();
            await _repo.SaveChangesAsync();

            // Ghi log hủy duyệt
            await LogChangeAsync(
                approval.DocumentID,
                uploaderId,
                "Approval",
                "Yêu cầu duyệt đã được hủy và quay lại trạng thái chờ duyệt."
            );

            return null;
        }




        public async Task<IEnumerable<DocumentApproval>> GetApprovalsByDocumentAsync(int documentId)
        {
            return (await _approvalRepo.GetAllAsync(a => a.DocumentID == documentId)).ToList();
        }


        // ================== MAPPING ==================
        private async Task<DocumentDetailVM> MapToDetailVM(Document d)
        {
            var uploader = await _userRepo.GetByIdAsync(d.UploaderID);
            var category = d.CategoryId.HasValue ? await _categoryRepo.GetByIdAsync(d.CategoryId.Value) : null;
            var docType = d.DocumentTypeId.HasValue ? await _docTypeRepo.GetByIdAsync(d.DocumentTypeId.Value) : null;

            // Lấy phòng ban gắn với tài liệu
            var docDeps = (await _docDeptRepo.GetAllAsync(dd => dd.DocumentId == d.DocumentId && dd.IsDeleted == false)).ToList();
            var departments = new List<DepartmentViewModel>();
            foreach (var dd in docDeps)
            {
                var dep = await _departmentRepo.GetByIdAsync(dd.DepartmentId);
                if (dep != null)
                {
                    departments.Add(new DepartmentViewModel
                    {
                        DepartmentId = dep.DepartmentId,
                        DepartmentName = dep.DepartmentName
                    });
                }
            }

            // 🔥 Lấy Approval mới nhất
            var latestApproval = (await _approvalRepo
                .GetAllAsync(a => a.DocumentID == d.DocumentId))
                .OrderByDescending(a => a.DateUploaded)
                .FirstOrDefault();

            return new DocumentDetailVM
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                Content = d.Content,
                CategoryId = d.CategoryId,
                CategoryName = category?.CategoryName,
                DocumentTypeId = d.DocumentTypeId,
                DocumentTypeName = docType?.DocumentTypeName,
                UploaderID = d.UploaderID,
                UploaderName = uploader?.FullName,
                FileName = d.FileName,
                FilePath = d.FilePath,
                FileType = d.FileType,
                FileSize = d.FileSize ?? 0,

                ApprovalId = latestApproval?.ApprovalID,   // ✅ Quan trọng
                ApprovalStatus = d.ApprovalStatus,
                ApprovalDate = d.ApprovalDate,
                IsDeleted = d.IsDeleted ?? false,
                CreatedAt = d.CreatedAt ?? DateTime.Now,
                UpdatedAt = d.UpdatedAt,
                Departments = departments
            };
        }

        // ================== XEM FILE ==================

        public async Task<(byte[] FileBytes, string ContentType, string FileName, bool Inline)?> GetFileAsync(int documentId, string serverRootPath)
        {
            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null || string.IsNullOrEmpty(doc.FilePath))
                return null;

            string fullPath = Path.Combine(serverRootPath, doc.FilePath.TrimStart('/').Replace("/", "\\"));
            if (!File.Exists(fullPath))
                return null;

            byte[] fileBytes = File.ReadAllBytes(fullPath);
            string ext = (doc.FileType ?? "").ToLower();

            string contentType;
            bool inline = false; // mặc định là tải xuống

            if (ext == ".pdf")
            {
                contentType = "application/pdf";
                inline = true; // hiển thị trên trình duyệt
            }
            else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                contentType = $"image/{ext.TrimStart('.')}";
                inline = true; // hiển thị trên trình duyệt
            }
            else if (ext == ".doc")
            {
                contentType = "application/msword";
            }
            else if (ext == ".docx")
            {
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
            else if (ext == ".xls")
            {
                contentType = "application/vnd.ms-excel";
            }
            else if (ext == ".xlsx")
            {
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else
            {
                contentType = "application/octet-stream";
            }

            return (fileBytes, contentType, doc.FileName, inline);
        }
    }
}
