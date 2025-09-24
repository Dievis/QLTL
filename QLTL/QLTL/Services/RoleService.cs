using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.RoleVM;
using QLTL.ViewModels.PermissionVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;

namespace QLTL.Services
{
    public class RoleService
    {
        private readonly IGenericRepository<Role> _roleRepo;
        private readonly IGenericRepository<Permission> _permissionRepo;
        private readonly IGenericRepository<RolePermission> _rolePermRepo;

        public RoleService(
            IGenericRepository<Role> roleRepo,
            IGenericRepository<Permission> permissionRepo,
            IGenericRepository<RolePermission> rolePermRepo)
        {
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
            _rolePermRepo = rolePermRepo;
        }

        // ================== LẤY DANH SÁCH CÓ PHÂN TRANG ==================
        public async Task<RoleIndexVM> GetAllRolesAsync(string search, bool? isDeleted, int pageIndex, int pageSize)
        {
            var filter = PredicateBuilder.New<Role>(true);

            if (!string.IsNullOrEmpty(search))
                filter = filter.And(r => r.RoleName.Contains(search) || r.Description.Contains(search));

            if (isDeleted.HasValue)
                filter = filter.And(r => r.IsDeleted == isDeleted.Value);

            var (items, total) = await _roleRepo.GetPagedAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(r => r.CreatedAt),
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return new RoleIndexVM
            {
                Items = items.Select(r => new RoleViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsDefault = r.IsDefault,
                    IsDeleted = r.IsDeleted,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = total,
                SearchTerm = search,
                IsDeleted = isDeleted
            };
        }

        // ================== LẤY CHI TIẾT ==================
        public async Task<RoleViewModel> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null) return null;

            return new RoleViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsDefault = role.IsDefault,
                IsDeleted = role.IsDeleted,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }

        // ================== THÊM MỚI ==================
        public async Task CreateRoleAsync(RoleViewModel model)
        {
            var entity = new Role
            {
                RoleName = model.RoleName,
                Description = model.Description,
                IsDefault = model.IsDefault,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            await _roleRepo.AddAsync(entity);
            await _roleRepo.SaveChangesAsync();
        }

        // ================== CẬP NHẬT ==================
        public async Task UpdateRoleAsync(RoleViewModel model)
        {
            var entity = await _roleRepo.GetByIdAsync(model.RoleId);
            if (entity == null) return;

            entity.RoleName = model.RoleName;
            entity.Description = model.Description;
            entity.IsDefault = model.IsDefault;
            entity.UpdatedAt = DateTime.Now;

            await _roleRepo.UpdateAsync(entity);
            await _roleRepo.SaveChangesAsync();
        }

        // ================== XÓA MỀM ==================
        public async Task SoftDeleteRoleAsync(int id)
        {
            var entity = await _roleRepo.GetByIdAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;

            await _roleRepo.UpdateAsync(entity);
            await _roleRepo.SaveChangesAsync();
        }

        // ================== LẤY PERMISSION CỦA ROLE ==================
        public async Task<IEnumerable<PermissionViewModel>> GetPermissionsByRoleAsync(int roleId)
        {
            var rolePerms = await _rolePermRepo.GetAllAsync(rp => rp.RoleId == roleId && !rp.IsDeleted);
            var permIds = rolePerms.Select(rp => rp.PermissionId).ToList();

            var permissions = await _permissionRepo.GetAllAsync(p => permIds.Contains(p.PermissionId) && !p.IsDeleted);
            return permissions.Select(p => new PermissionViewModel
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

        // ================== GÁN PERMISSION CHO ROLE ==================
        public async Task AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            var current = await _rolePermRepo.GetAllAsync(rp => rp.RoleId == roleId);

            // Xóa những permission không còn trong danh sách mới
            foreach (var rp in current)
            {
                if (!permissionIds.Contains(rp.PermissionId))
                {
                    rp.IsDeleted = true;
                    rp.UpdatedAt = DateTime.Now;
                    await _rolePermRepo.UpdateAsync(rp);
                }
            }

            // Thêm những permission mới chưa tồn tại
            foreach (var pid in permissionIds)
            {
                if (!current.Any(rp => rp.PermissionId == pid && !rp.IsDeleted))
                {
                    var rp = new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = pid,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now
                    };
                    await _rolePermRepo.AddAsync(rp);
                }
            }

            await _rolePermRepo.SaveChangesAsync();
        }
    }
}
