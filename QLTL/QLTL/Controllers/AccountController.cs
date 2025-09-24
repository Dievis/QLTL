using QLTL.Attributes;
using QLTL.Helpers;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.Services;
using QLTL.ViewModels.AccountVM;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthorizeService _authService;

        public AccountController()
        {
            _authService = new AuthorizeService(
                new GenericRepository<User>(new QLTL_NEWEntities()),
                new GenericRepository<Role>(new QLTL_NEWEntities()),
                new GenericRepository<UserRole>(new QLTL_NEWEntities()),
                new GenericRepository<RolePermission>(new QLTL_NEWEntities()),
                new GenericRepository<Permission>(new QLTL_NEWEntities()),
                new GenericRepository<Department>(new QLTL_NEWEntities())
            );
        }

        public AccountController(AuthorizeService authService)
        {
            _authService = authService;
        }

        // ================== ĐĂNG NHẬP ==================
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                // Đã đăng nhập thì không cho vào Login nữa, chuyển về Home
                return RedirectToAction("Index", "Home");
            }

            var reason = Request.QueryString["reason"];
            if (!string.IsNullOrEmpty(reason))
            {
                if (reason == "disabled")
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị vô hiệu hóa.";
                }
                else if (reason == "deleted")
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị xóa.";
                }
                else if (reason == "logout")
                {
                    ViewBag.Success = "Bạn đã đăng xuất thành công.";
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _authService.LoginAsync(username, password);

            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View();
            }

            var user = result.User;

            var perms = await _authService.GetPermissionsByUserIdAsync(user.UserId);
            var role = await _authService.GetMainRoleByUserIdAsync(user.UserId);

            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Permissions"] = perms;
            Session["UserAvatar"] = user.Avatar;
            Session["Role"] = role;
            Session["IsSuperAdmin"] = user.IsSuperAdmin;

            return RedirectToAction("Index", "Home");
        }


        // ================== ĐĂNG XUẤT ==================

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // ================== QUÊN MẬT KHẨU ==================
        public ActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            try
            {
                var token = await _authService.GenerateResetPasswordTokenAsync(email);

                // Gửi mail
                string resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Url.Scheme);
                string body = $"<p>Bấm vào link để đặt lại mật khẩu:</p><a href='{resetLink}'>Đặt lại mật khẩu</a>";

                await MailHelper.SendMailAsync(email, "Đặt lại mật khẩu", body);

                ViewBag.Message = "Vui lòng kiểm tra email để đặt lại mật khẩu";
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View();
        }

        // ================== ĐẶT LẠI MẬT KHẨU ==================
        public ActionResult ResetPassword(string token) => View(model: token);

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string token, string newPassword)
        {
            var success = await _authService.ResetPasswordAsync(token, newPassword);
            if (!success)
            {
                ViewBag.Error = "Token không hợp lệ hoặc đã hết hạn";
                return View(model: token);
            }

            ViewBag.Message = "Đổi mật khẩu thành công";
            return RedirectToAction("Login");
        }

        // ================== CHECK MÃ NHÂN VIÊN ==================
        public ActionResult CheckEmployee()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CheckEmployee(string employeeCode)
        {
            var user = await _authService.FindByEmployeeCodeAsync(employeeCode);
            if (user == null)
            {
                ViewBag.Error = "Mã nhân viên không tồn tại, vui lòng liên hệ quản trị viên.";
                return View();
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                ViewBag.Error = "Tài khoản này đã có email. Vui lòng dùng chức năng Quên mật khẩu.";
                return View();
            }

            // Lưu tạm Id + Username để sang trang ResetCredentials
            TempData["UserId"] = user.UserId;
            TempData["Username"] = user.Username;

            return RedirectToAction("ResetCredentials");
        }

        // ================== RESET EMAIL + PASSWORD ==================
        public ActionResult ResetCredentials()
        {
            if (TempData["UserId"] == null) return RedirectToAction("CheckEmployee");

            var vm = new ResetCredentialsVM
            {
                UserId = (int)TempData["UserId"],
                Username = TempData["Username"].ToString()
            };
            return View(vm);
        }


        [HttpPost]
        public async Task<ActionResult> ResetCredentials(int userId, string email, string password)
        {
            var user = await _authService.FindByUserIdAsync(userId);

            if (user == null || user.IsDeleted)
            {
                ViewBag.Error = "Người dùng không tồn tại";
                return View();
            }

            // Chỉ cho reset nếu user chưa có email
            if (!string.IsNullOrEmpty(user.Email))
            {
                ViewBag.Error = "Tài khoản này đã có email. Vui lòng dùng chức năng Quên mật khẩu.";
                return View();
            }

            bool updated = await _authService.UpdateEmailAndPasswordAsync(userId, email, password);
            if (updated)
            {
                TempData["Message"] = "Cập nhật thành công. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Cập nhật thất bại.";
            return View();
        }

        // ================== ACCESS DENIED ==================
        public ActionResult AccessDenied() => View();


    }
}
