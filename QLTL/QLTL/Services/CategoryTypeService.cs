using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.CategoryTypeVM;
using System;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;

namespace QLTL.Services
{
    public class CategoryTypeService
    {
        private readonly IGenericRepository<CategoryType> _repo;

        public CategoryTypeService(IGenericRepository<CategoryType> repo)
        {
            _repo = repo;
        }

        // ================== LẤY DANH SÁCH CÓ PHÂN TRANG ==================
        public async Task<CategoryTypeIndexVM> GetAllAsync(string search, bool? isDeleted, int pageIndex, int pageSize)
        {
            var filter = PredicateBuilder.New<CategoryType>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(c => c.CategoryTypeName.Contains(search) || c.Description.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(c => c.IsDeleted == isDeleted.Value);

            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(c => c.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return new CategoryTypeIndexVM
            {
                Items = items.Select(c => new CategoryTypeViewModel
                {
                    CategoryTypeId = c.CategoryTypeId,
                    CategoryTypeName = c.CategoryTypeName,
                    Description = c.Description,
                    IsDeleted = c.IsDeleted ?? false,
                    CreatedAt = c.CreatedAt ?? DateTime.Now,
                    UpdatedAt = c.UpdatedAt
                }).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }

        // ================== LẤY CHI TIẾT ==================
        public async Task<CategoryTypeViewModel> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return null;

            return new CategoryTypeViewModel
            {
                CategoryTypeId = c.CategoryTypeId,
                CategoryTypeName = c.CategoryTypeName,
                Description = c.Description,
                IsDeleted = c.IsDeleted ?? false,
                CreatedAt = c.CreatedAt ?? DateTime.Now,
                UpdatedAt = c.UpdatedAt
            };
        }

        // ================== THÊM MỚI ==================
        public async Task CreateAsync(CategoryTypeViewModel model)
        {
            var entity = new CategoryType
            {
                CategoryTypeName = model.CategoryTypeName,
                Description = model.Description,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        // ================== CẬP NHẬT ==================
        public async Task UpdateAsync(CategoryTypeViewModel model)
        {
            var entity = await _repo.GetByIdAsync(model.CategoryTypeId);
            if (entity == null) return;

            entity.CategoryTypeName = model.CategoryTypeName;
            entity.Description = model.Description;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        // ================== XÓA MỀM ==================
        public async Task SoftDeleteAsync(int id)
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
