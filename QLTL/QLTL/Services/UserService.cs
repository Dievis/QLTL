using LinqKit;
using QLTL.Helpers;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.UserVM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace QLTL.Services
{
    public class UserService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Role> _roleRepo;
        private readonly IGenericRepository<UserRole> _userRoleRepo;
        private readonly IGenericRepository<Department> _departmentRepo;

        public UserService(
            IGenericRepository<User> userRepo,
            IGenericRepository<Role> roleRepo,
            IGenericRepository<UserRole> userRoleRepo,
            IGenericRepository<Department> departmentRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _departmentRepo = departmentRepo;
        }

        // ================== LẤY DANH SÁCH CÓ PHÂN TRANG ==================
        public async Task<UserIndexVM> GetAllAsync(string search, bool? isDeleted, int pageIndex, int pageSize)
        {
            using (var db = new QLTL_NEWEntities())
            {
                var query = db.Users.Include("Department")
                                    .Where(u => !u.IsSuperAdmin) // ✅ Ẩn luôn SuperAdmin
                                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(u =>
                        u.Username.Contains(search) ||
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search) ||
                        u.Phone.Contains(search));

                if (isDeleted.HasValue)
                    query = query.Where(u => u.IsDeleted == isDeleted.Value);

                var total = await query.CountAsync();

                var items = await query.OrderByDescending(u => u.CreatedAt)
                                       .Skip((pageIndex - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

                return new UserIndexVM
                {
                    Items = items.Select(u => new UserViewModel
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        FullName = u.FullName,
                        Email = u.Email,
                        Phone = u.Phone,
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.Department?.DepartmentName,
                        IsSuperAdmin = u.IsSuperAdmin,
                        IsActive = u.IsActive,
                        IsDeleted = u.IsDeleted,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    }).ToList(),
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalRecords = total,
                    SearchTerm = search,
                    IsDeleted = isDeleted
                };
            }
        }

        // ================== LẤY DANH SÁCH ROLE ==================
        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            return await _roleRepo.GetAllAsync(r => !r.IsDeleted && !r.IsDefault);
            // Nếu bạn muốn hiển thị cả role mặc định thì bỏ "!r.IsDefault"
        }



        // ================== LẤY CHI TIẾT ==================
        public async Task<UserViewModel> GetByIdAsync(int id)
        {
            var u = await _userRepo.GetByIdAsync(id);
            if (u == null) return null;

            var dept = u.DepartmentId.HasValue ? await _departmentRepo.GetByIdAsync(u.DepartmentId.Value) : null;

            return new UserViewModel
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                DepartmentId = u.DepartmentId,
                DepartmentName = dept?.DepartmentName,
                IsSuperAdmin = u.IsSuperAdmin,
                IsActive = u.IsActive,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            };
        }

        // ================== TẠO MỚI ==================
        public async Task CreateAsync(UserViewModel model)
        {
            if ((await _userRepo.GetAllAsync(u => u.Username == model.Username)).Any())
                throw new Exception("Tên đăng nhập đã tồn tại");

            var user = new User
            {
                Username = model.Username,
                PasswordHash = PasswordHelper.HashPassword(model.Password), // cần thêm Password trong VM khi tạo
                FullName = model.FullName,
                DepartmentId = model.DepartmentId,
                DateJoined = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            // Gán role được chọn
            if (model.SelectedRoleIds != null && model.SelectedRoleIds.Any())
            {
                foreach (var roleId in model.SelectedRoleIds)
                {
                    var ur = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = roleId,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };
                    await _userRoleRepo.AddAsync(ur);
                }
                await _userRoleRepo.SaveChangesAsync();
            }
        }

        // ================== LẤY DANH SÁCH PHÒNG BAN ==================
        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            return await _departmentRepo.GetAllAsync();
        }

        // ================== VÔ HIỆU HÓA / KÍCH HOẠT USER ==================
        public async Task<bool> ToggleActiveAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new Exception("Người dùng không tồn tại");

            if (user.IsSuperAdmin) throw new Exception("Không thể thay đổi trạng thái SuperAdmin");

            user.IsActive = !user.IsActive;   // Đảo trạng thái
            user.UpdatedAt = DateTime.Now;

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            return user.IsActive; // trả về trạng thái hiện tại sau khi đảo
        }

        // ================== CẬP NHẬT ==================
        public async Task UpdateAsync(UserViewModel model)
        {
            var user = await _userRepo.GetByIdAsync(model.UserId);
            if (user == null) return;

            user.Username = model.Username;
            user.FullName = model.FullName;
            user.DepartmentId = model.DepartmentId;
            user.IsSuperAdmin = model.IsSuperAdmin;
            user.IsActive = model.IsActive;
            user.IsDeleted = model.IsDeleted;
            user.UpdatedAt = DateTime.Now;

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            if (model.SelectedRoleIds != null && model.SelectedRoleIds.Any())
            {
                using (var db = new QLTL.Models.QLTL_NEWEntities())
                {
                    foreach (var roleId in model.SelectedRoleIds)
                    {
                        // Kiểm tra trực tiếp trong database
                        var exists = await db.UserRoles
                            .Where(x => x.UserId == user.UserId && x.RoleId == roleId && !x.IsDeleted)
                            .AnyAsync();

                        if (!exists)
                        {
                            // Chỉ add nếu chưa có
                            var newUserRole = new UserRole
                            {
                                UserId = user.UserId,
                                RoleId = roleId,
                                CreatedAt = DateTime.Now,
                                IsDeleted = false
                            };
                            db.UserRoles.Add(newUserRole);
                        }
                        // Nếu exists == true => bỏ qua, không Add nữa
                    }

                    await db.SaveChangesAsync();
                }
            }

        }




        // ================== XÓA MỀM ==================
        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;

            await _userRepo.UpdateAsync(entity);
            await _userRepo.SaveChangesAsync();
        }
    }
}
