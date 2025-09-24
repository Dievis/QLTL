using QLTL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QLTL.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizeCustomAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Danh sách Permission yêu cầu, phân tách bằng dấu phẩy
        /// </summary>
        public string Permissions { get; set; }

        /// <summary>
        /// Chế độ kiểm tra quyền:
        /// - true = user phải có TẤT CẢ quyền (AND)
        /// - false = chỉ cần có 1 trong các quyền (OR)
        /// </summary>
        public bool RequireAll { get; set; } = true;

        /// <summary>
        /// Trang redirect khi bị cấm (403)
        /// </summary>
        public string AccessDeniedUrl { get; set; } = "~/Account/AccessDenied";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            // Chưa login
            if (httpContext.Session["UserId"] == null)
                return false;

            int userId = (int)httpContext.Session["UserId"];

            // Kiểm tra trạng thái thực tế trên DB
            using (var db = new QLTL_NEWEntities())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    // Hủy session + cookie để force logout ngay
                    httpContext.Session.Clear();
                    try { FormsAuthentication.SignOut(); } catch { }
                    return false;
                }
            }

            // SuperAdmin bypass tất cả quyền
            if (httpContext.Session["IsSuperAdmin"] is bool isSuperAdmin && isSuperAdmin)
                return true;

            // Nếu không yêu cầu quyền cụ thể -> chỉ cần login
            if (string.IsNullOrWhiteSpace(Permissions))
                return true;

            // Lấy danh sách quyền trong Session
            var userPermissions = httpContext.Session["Permissions"] as List<string>;
            if (userPermissions == null || !userPermissions.Any())
                return false;

            // Chuẩn hóa yêu cầu
            var required = Permissions.Split(',')
                                      .Select(p => p.Trim())
                                      .Where(p => !string.IsNullOrEmpty(p))
                                      .ToList();

            if (!required.Any())
                return true;

            if (RequireAll)
            {
                // Cần tất cả
                return required.All(r => userPermissions.Contains(r, StringComparer.OrdinalIgnoreCase));
            }
            else
            {
                // Chỉ cần 1 trong số đó
                return required.Any(r => userPermissions.Contains(r, StringComparer.OrdinalIgnoreCase));
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            // đảm bảo session đã clear nếu cần
            httpContext.Session.Clear();

            if (httpContext.Session["UserId"] == null)
            {
                // chưa đăng nhập
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                // đã đăng nhập nhưng thiếu quyền
                if (!string.IsNullOrEmpty(AccessDeniedUrl))
                {
                    filterContext.Result = new RedirectResult(AccessDeniedUrl);
                }
                else
                {
                    filterContext.Result = new HttpStatusCodeResult(403, "Bạn không có quyền thực hiện hành động này");
                }
            }
        }
    }
}
