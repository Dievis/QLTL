using QLTL.Helpers;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.ViewModels.AccountVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Services
{
    public class AuthorizeService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Role> _roleRepo;
        private readonly IGenericRepository<UserRole> _userRoleRepo;
        private readonly IGenericRepository<RolePermission> _rolePermRepo;
        private readonly IGenericRepository<Permission> _permRepo;
        private readonly IGenericRepository<Department> _departmentRepo;



        public AuthorizeService(
            IGenericRepository<User> userRepo,
            IGenericRepository<Role> roleRepo,
            IGenericRepository<UserRole> userRoleRepo,
            IGenericRepository<RolePermission> rolePermRepo,
            IGenericRepository<Permission> permRepo,
            IGenericRepository<Department> departmentRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _rolePermRepo = rolePermRepo;
            _permRepo = permRepo;
            _departmentRepo = departmentRepo;
        }

        // ================== ĐĂNG KÝ ==================
        public async Task<User> RegisterAsync(string username, string password, string fullName, int? departmentId = null, bool isSuperAdmin = false, bool isActive = true)
        {
            // Check tồn tại username
            var exists = (await _userRepo.GetAllAsync(u => u.Username == username)).FirstOrDefault();
            if (exists != null) throw new Exception("Tên đăng nhập đã tồn tại");

            // Khởi tạo context để dùng CodeHelper (nếu chưa có dbcontext inject sẵn)
            using (var db = new QLTL_NEWEntities())
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = PasswordHelper.HashPassword(password), // bcrypt
                    FullName = fullName,
                    EmployeeCode = CodeHelper.GenerateEmployeeCode(db), // 🔥 tự sinh
                    DepartmentId = departmentId == 0 ? null : departmentId,
                    IsSuperAdmin = isSuperAdmin,
                    DateJoined = DateTime.Now,
                    IsActive = isActive,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();

                // Gán role mặc định (USER)
                var defaultRole = (await _roleRepo.GetAllAsync(r => r.IsDefault && !r.IsDeleted)).FirstOrDefault();
                if (defaultRole != null)
                {
                    var ur = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = defaultRole.RoleId,
                        CreatedAt = DateTime.Now
                    };
                    await _userRoleRepo.AddAsync(ur);
                    await _userRoleRepo.SaveChangesAsync();
                }

                return user;
            }
        }

        // ================== ĐĂNG NHẬP ==================
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var user = (await _userRepo.GetAllAsync(u => u.Username == username))
                        .FirstOrDefault();

            if (user == null)
                return new LoginResult { Success = false, Message = "Tài khoản không tồn tại." };

            if (user.IsDeleted)
                return new LoginResult { Success = false, Message = "Tài khoản đã bị xóa." };

            if (!user.IsActive)
                return new LoginResult { Success = false, Message = "Tài khoản đã bị vô hiệu hóa." };

            var check = PasswordHelper.TryVerifyPassword(password, user.PasswordHash);
            if (check == PasswordCheckResult.MatchBcrypt || check == PasswordCheckResult.MatchLegacyPlain)
            {
                // Nếu match legacy/plain thì auto nâng cấp sang bcrypt
                if (check == PasswordCheckResult.MatchLegacyPlain)
                {
                    user.PasswordHash = PasswordHelper.HashPassword(password);
                    await _userRepo.UpdateAsync(user);
                    await _userRepo.SaveChangesAsync();
                }

                return new LoginResult { Success = true, User = user };
            }

            return new LoginResult { Success = false, Message = "Sai mật khẩu." };
        }


        // ================== LẤY QUYỀN CỦA USER ==================
        public async Task<List<string>> GetPermissionsByUserIdAsync(int userId)
        {
            var roleIds = (await _userRoleRepo.GetAllAsync(ur => ur.UserId == userId))
                          .Select(ur => ur.RoleId).ToList();

            if (!roleIds.Any()) return new List<string>();

            var perms = (from rp in await _rolePermRepo.GetAllAsync(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
                         join p in await _permRepo.GetAllAsync(p => !p.IsDeleted) on rp.PermissionId equals p.PermissionId
                         select p.PermissionName)
                         .Distinct()
                         .ToList();

            return perms;
        }

        // ================== LẤY VAI TRÒ CỦA USER ==================

        public async Task<string> GetMainRoleByUserIdAsync(int userId)
        {
            var role = (await _userRoleRepo.GetAllAsync(ur => ur.UserId == userId))
                        .Select(ur => ur.Role.RoleName)
                        .FirstOrDefault();
            return role ?? "User";
        }

        // ================== LẤY PHÒNG BAN CỦA USER ==================
        public async Task<List<SelectListItem>> GetAllDepartmentsAsync()
        {
            var list = await _departmentRepo.GetAllAsync(d => !d.IsDeleted);
            return list.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }

        // ================== TÌM USER THEO ID ==================
        public async Task<User> FindByUserIdAsync(int userId)
        {
            return await _userRepo.GetByIdAsync(userId);
        }

        // ================== TÌM USER THEO MÃ NHÂN VIÊN ==================
        public async Task<User> FindByEmployeeCodeAsync(string employeeCode)
        {
            var user = (await _userRepo.GetAllAsync(u => u.EmployeeCode == employeeCode && !u.IsDeleted))
                       .FirstOrDefault();
            return user;
        }

        // ================== CẬP NHẬT EMAIL + PASSWORD ==================
        public async Task<bool> UpdateEmailAndPasswordAsync(int userId, string email, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null || user.IsDeleted) return false;

            user.Email = email;
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            return true;
        }



        // ================== QUÊN MẬT KHẨU (TẠO TOKEN + GỬI MAIL) ==================
        public async Task<string> GenerateResetPasswordTokenAsync(string email)
        {
            var user = (await _userRepo.GetAllAsync(u => u.Email == email && !u.IsDeleted)).FirstOrDefault();
            if (user == null) throw new Exception("Email không tồn tại");

            string token = Guid.NewGuid().ToString();
            user.ResetPasswordToken = token;
            user.ResetPasswordExpiry = DateTime.Now.AddHours(1);

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            // gửi email
            string resetLink = $"https://yourapp.com/Account/ResetPassword?token={token}";
            string body = $"<p>Click vào link để đặt lại mật khẩu:</p><p><a href='{resetLink}'>Reset Password</a></p>";

            await MailHelper.SendMailAsync(user.Email, "Đặt lại mật khẩu", body);

            return token;
        }

        // ================== ĐẶT LẠI MẬT KHẨU ==================
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = (await _userRepo.GetAllAsync(u => u.ResetPasswordToken == token && u.ResetPasswordExpiry > DateTime.Now))
                        .FirstOrDefault();
            if (user == null) return false;

            user.PasswordHash = PasswordHelper.HashPassword(newPassword); // bcrypt
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            user.UpdatedAt = DateTime.Now;

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            return true;
        }
    }
}
