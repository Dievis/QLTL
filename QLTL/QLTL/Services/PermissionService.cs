using LinqKit;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.PermissionVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLTL.Services
{
    public class PermissionService
    {
        private readonly IGenericRepository<Permission> _repo;

        public PermissionService(IGenericRepository<Permission> repo)
        {
            _repo = repo;
        }

        // ================== LẤY DANH SÁCH CÓ PHÂN TRANG ==================
        public async Task<PermissionIndexVM> GetAllPermissionsAsync(string search = null, bool? isDeleted = null, int pageIndex = 1, int pageSize = 10)
        {
            var filter = PredicateBuilder.New<Permission>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(p => p.PermissionName.Contains(search) || p.Description.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(p => p.IsDeleted == isDeleted.Value);

            var (items, total) = await _repo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(p => p.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return new PermissionIndexVM
            {
                Items = items.Select(p => new PermissionViewModel
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Description = p.Description,
                    IsDefault = p.IsDefault,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }

        // ================== LẤY DANH SÁCH KHÔNG PHÂN TRANG ==================
        public async Task<IEnumerable<PermissionViewModel>> GetAllPermissionsNoPagingAsync()
        {
            var perms = await _repo.GetAllAsync(p => !p.IsDeleted);
            return perms.Select(p => new PermissionViewModel
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                Description = p.Description,
                IsDefault = p.IsDefault,
                IsDeleted = p.IsDeleted,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }


        // ================== LẤY CHI TIẾT ==================
        public async Task<PermissionViewModel> GetPermissionByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return null;

            return new PermissionViewModel
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                Description = p.Description,
                IsDefault = p.IsDefault,
                IsDeleted = p.IsDeleted,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        // ================== THÊM MỚI ==================
        public async Task<string> CreatePermissionAsync(PermissionViewModel model)
        {
            var exists = await _repo.GetAllAsync(p => p.PermissionName == model.PermissionName);
            if (exists.Any())
            {
                return "Permission đã tồn tại.";
            }

            var entity = new Permission
            {
                PermissionName = model.PermissionName,
                Description = model.Description,
                IsDefault = model.IsDefault,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return null; // null = thêm thành công
        }

        // ================== CẬP NHẬT ==================
        public async Task UpdatePermissionAsync(PermissionViewModel model)
        {
            var entity = await _repo.GetByIdAsync(model.PermissionId);
            if (entity == null) return;

            entity.PermissionName = model.PermissionName;
            entity.Description = model.Description;
            entity.IsDefault = model.IsDefault;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        // ================== XÓA MỀM ==================
        public async Task SoftDeletePermissionAsync(int id)
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
