using LinqKit;
using QLTL.Helpers;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.UserVM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
                var query = db.Users
                              .Include("Department")
                              .Include("UserRoles.Role")   // load role
                              .Where(u => !u.IsSuperAdmin) // ẩn SuperAdmin
                              .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u =>
                        u.Username.Contains(search) ||
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search) ||
                        u.Phone.Contains(search));
                }

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
                        UpdatedAt = u.UpdatedAt,

                        // 🔥 map roles cho từng user
                        RoleNames = u.UserRoles
                            .Where(ur => !ur.IsDeleted)
                            .Select(ur => ur.Role.RoleName)
                            .ToList()
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

        // ================== LẤY DANH SÁCH PHÒNG BAN ==================
        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            return await _departmentRepo.GetAllAsync();
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

            using (var db = new QLTL_NEWEntities())
            {
                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    FullName = model.FullName,
                    DepartmentId = model.DepartmentId,
                    DateJoined = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,

                    // 🔥 Gọi helper sinh mã nhân viên duy nhất
                    EmployeeCode = CodeHelper.GenerateEmployeeCode(db),

                    // Nếu muốn có thể lưu luôn email/phone (nếu có)
                    Email = model.Email,
                    Phone = model.Phone
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();

                // Nếu không chọn role -> gán role mặc định "User"
                var roleIds = model.SelectedRoleIds != null && model.SelectedRoleIds.Any()
                    ? model.SelectedRoleIds.Distinct().ToList()
                    : (await _roleRepo.GetAllAsync(r => r.RoleName == "User" && !r.IsDeleted))
                          .Select(r => r.RoleId)
                          .ToList();

                foreach (var roleId in roleIds)
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

            // Cập nhật role
            var currentRoles = await _userRoleRepo.GetAllAsync(x => x.UserId == user.UserId && !x.IsDeleted);
            var currentRoleIds = currentRoles.Select(x => x.RoleId).ToList();
            var newRoleIds = model.SelectedRoleIds?.Distinct().ToList() ?? new List<int>();

            // 1. Xóa role không còn
            foreach (var ur in currentRoles)
            {
                if (!newRoleIds.Contains(ur.RoleId))
                {
                    ur.IsDeleted = true;
                    ur.DeletedAt = DateTime.Now;
                    await _userRoleRepo.UpdateAsync(ur);
                }
            }

            // 2. Thêm role mới
            foreach (var roleId in newRoleIds)
            {
                if (!currentRoleIds.Contains(roleId))
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
            }

            await _userRoleRepo.SaveChangesAsync();
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

        // ================== CẬP NHẬT THÔNG TIN CÁ NHÂN ==================

        public async Task<UserProfileVM> GetProfileAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileVM
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.Avatar,
                DepartmentName = user.Department?.DepartmentName,
                EmployeeCode = user.EmployeeCode,
                RoleNames = user.UserRoles.Where(x => !x.IsDeleted).Select(x => x.Role.RoleName).ToList()
            };
        }

        public async Task UpdateProfileAsync(UserProfileVM model, HttpPostedFileBase avatarFile, string serverPath)
        {
            var user = await _userRepo.GetByIdAsync(model.UserId);
            if (user == null) throw new Exception("Người dùng không tồn tại");

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            // Nếu nhập mật khẩu mới thì hash lại
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                    throw new Exception("Mật khẩu xác nhận không khớp");

                user.PasswordHash = PasswordHelper.HashPassword(model.Password);
            }

            // Upload ảnh đại diện
            if (avatarFile != null && avatarFile.ContentLength > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                string relativePath = "/Content/uploads/avatars/" + fileName;
                string absolutePath = Path.Combine(serverPath, "Content/uploads/avatars", fileName);

                // Tạo thư mục nếu chưa có
                var dir = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                avatarFile.SaveAs(absolutePath);
                user.Avatar = relativePath;
                model.AvatarUrl = relativePath; // để cập nhật session sau này
            }

            user.UpdatedAt = DateTime.Now;

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();
        }


    }
}
