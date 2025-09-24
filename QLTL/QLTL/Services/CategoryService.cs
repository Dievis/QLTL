using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.CategoryVM;
using System;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using System.Web.Mvc;

namespace QLTL.Services
{
    public class CategoryService
    {
        private readonly IGenericRepository<Category> _repo;
        private readonly CategoryTypeService _typeService; // thêm CategoryTypeService vào CategoryService để lấy tên loại

        public CategoryService(IGenericRepository<Category> repo, CategoryTypeService typeService)
        {
            _repo = repo;
            _typeService = typeService;
        }

        public async Task<CategoryIndexVM> GetAllAsync(string search, bool? isDeleted, int pageIndex, int pageSize)
        {
            // Build filter cho Category
            var filter = PredicateBuilder.New<Category>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(c => c.CategoryName.Contains(search) || c.Description.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(c => c.IsDeleted == isDeleted.Value);

            // Lấy danh sách phân trang của Category
            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(c => c.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            // Lấy tất cả loại danh mục (CategoryType) để map tên
            var typesVM = await _typeService.GetAllAsync(null, false, 1, 100);
            var typeDict = typesVM.Items.ToDictionary(t => t.CategoryTypeId, t => t.CategoryTypeName);

            // Map Category sang CategoryViewModel, gán CategoryTypeName
            var vmItems = items.Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryTypeId = c.CategoryTypeId,
                CategoryTypeName = typeDict.ContainsKey(c.CategoryTypeId) ? typeDict[c.CategoryTypeId] : "",
                Description = c.Description,
                IsDeleted = c.IsDeleted ?? false,
                CreatedAt = c.CreatedAt ?? DateTime.Now,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return new CategoryIndexVM
            {
                Items = vmItems,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }


        public async Task<CategoryViewModel> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return null;

            return new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryTypeId = c.CategoryTypeId,
                Description = c.Description,
                IsDeleted = c.IsDeleted ?? false,
                CreatedAt = c.CreatedAt ?? DateTime.Now,
                UpdatedAt = c.UpdatedAt
            };
        }

        public async Task CreateAsync(CategoryViewModel model)
        {

            var entity = new Category
            {
                CategoryName = model.CategoryName,
                CategoryTypeId = model.CategoryTypeId,
                Description = model.Description,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryViewModel model)
        {
            var entity = await _repo.GetByIdAsync(model.CategoryId);
            if (entity == null) return;

            entity.CategoryName = model.CategoryName;
            entity.CategoryTypeId = model.CategoryTypeId;
            entity.Description = model.Description;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task<CategoryViewModel> PrepareCreateVMAsync()
        {
            var types = await _typeService.GetAllAsync(null, false, 1, 100); // _typeService là CategoryTypeService
            return new CategoryViewModel
            {
                CategoryTypeList = types.Items.Select(t => new SelectListItem
                {
                    Value = t.CategoryTypeId.ToString(),
                    Text = t.CategoryTypeName
                })
            };
        }

        public async Task<CategoryViewModel> PrepareEditVMAsync(int id)
        {
            var vm = await GetByIdAsync(id);
            if (vm == null) return null;

            var types = await _typeService.GetAllAsync(null, false, 1, 100);
            vm.CategoryTypeList = types.Items.Select(t => new SelectListItem
            {
                Value = t.CategoryTypeId.ToString(),
                Text = t.CategoryTypeName,
                Selected = t.CategoryTypeId == vm.CategoryTypeId
            });

            return vm;
        }

    }
}
