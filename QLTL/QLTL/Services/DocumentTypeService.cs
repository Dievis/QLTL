using LinqKit;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.DocumentTypeVM;
using QLTL.ViewModels.DocumentVM;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLTL.Services
{
    public class DocumentTypeService
    {
        private readonly IGenericRepository<DocumentType> _repo;

        public DocumentTypeService(IGenericRepository<DocumentType> repo)
        {
            _repo = repo;
        }

        // Lấy danh sách có phân trang
        public async Task<DocumentTypeIndexVM> GetAllDocumentTypesAsync(string search = null, bool? isDeleted = null, int pageIndex = 1, int pageSize = 10)
        {
            var filter = PredicateBuilder.New<DocumentType>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(d => d.DocumentTypeName.Contains(search) || d.Description.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(d => d.IsDeleted == isDeleted.Value);

            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(d => d.DocumentTypeId),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return new DocumentTypeIndexVM
            {
                Items = items.Select(d => new DocumentTypeViewModel
                {
                    DocumentTypeId = d.DocumentTypeId,
                    DocumentTypeName = d.DocumentTypeName,
                    Description = d.Description,
                    IsDeleted = d.IsDeleted ?? false,
                    CreatedAt = d.CreatedAt ?? DateTime.Now,
                    UpdatedAt = d.UpdatedAt
                }).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }

        public async Task<DocumentTypeViewModel> GetDocumentTypeByIdAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return null;

            return new DocumentTypeViewModel
            {
                DocumentTypeId = d.DocumentTypeId,
                DocumentTypeName = d.DocumentTypeName,
                Description = d.Description,
                IsDeleted = d.IsDeleted ?? false,
                CreatedAt = d.CreatedAt ?? DateTime.Now,
                UpdatedAt = d.UpdatedAt
            };
        }

        public async Task CreateDocumentTypeAsync(DocumentTypeViewModel model)
        {
            var entity = new DocumentType
            {
                DocumentTypeName = model.DocumentTypeName,
                Description = model.Description,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateDocumentTypeAsync(DocumentTypeViewModel model)
        {
            var entity = await _repo.GetByIdAsync(model.DocumentTypeId);
            if (entity == null) return;

            entity.DocumentTypeName = model.DocumentTypeName;
            entity.Description = model.Description;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task SoftDeleteDocumentTypeAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
