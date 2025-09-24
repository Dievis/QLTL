using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.DepartmentVM;
using System;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;

namespace QLTL.Services
{
    public class DepartmentService
    {
        private readonly IGenericRepository<Department> _repo;

        public DepartmentService(IGenericRepository<Department> repo)
        {
            _repo = repo;
        }

        // ================== LẤY DANH SÁCH CÓ PHÂN TRANG ==================
        public async Task<DepartmentIndexVM> GetAllAsync(string search, bool? isDeleted, int pageIndex, int pageSize)
        {
            // Tạo filter động
            var filter = PredicateBuilder.New<Department>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(d => d.DepartmentName.Contains(search) || d.Description.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(d => d.IsDeleted == isDeleted.Value);

            // Phân trang và lấy tổng số bản ghi
            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(d => d.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return new DepartmentIndexVM
            {
                Items = items.Select(d => new DepartmentViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    IsDeleted = d.IsDeleted,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }

        // ================== LẤY CHI TIẾT ==================
        public async Task<DepartmentViewModel> GetByIdAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return null;

            return new DepartmentViewModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsDeleted = d.IsDeleted,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            };
        }

        // ================== THÊM MỚI ==================
        public async Task CreateAsync(DepartmentViewModel model)
        {
            var entity = new Department
            {
                DepartmentName = model.DepartmentName,
                Description = model.Description,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        // ================== CẬP NHẬT ==================
        public async Task UpdateAsync(DepartmentViewModel model)
        {
            var entity = await _repo.GetByIdAsync(model.DepartmentId);
            if (entity == null) return;

            entity.DepartmentName = model.DepartmentName;
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
