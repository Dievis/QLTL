using QLTL.Attributes;
using QLTL.Repositories;
using QLTL.Services;
using QLTL.ViewModels.UserVM;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class UserController : Controller
    {
        private readonly UserService _service;

        public UserController()
        {
            var db = new Models.QLTL_NEWEntities();
            var userRepo = new GenericRepository<Models.User>(db);
            var roleRepo = new GenericRepository<Models.Role>(db);
            var userRoleRepo = new GenericRepository<Models.UserRole>(db);
            var deptRepo = new GenericRepository<Models.Department>(db);

            _service = new UserService(userRepo, roleRepo, userRoleRepo, deptRepo);
        }

        // ================== DANH SÁCH ==================
        [AuthorizeCustom(Permissions = "User.View", RequireAll = true)] 

        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var vm = await _service.GetAllAsync(search, isDeleted, page, pageSize);
            return View(vm);
        }

        // ================== CHI TIẾT ==================
        public async Task<ActionResult> Details(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        // ================== TẠO MỚI ==================
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ViewBag.Departments = await _service.GetDepartmentsAsync();
            ViewBag.Roles = await _service.GetRolesAsync(); // cần thêm hàm GetRolesAsync
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(vm);
                return RedirectToAction("Index");
            }

            ViewBag.Departments = await _service.GetDepartmentsAsync();
            ViewBag.Roles = await _service.GetRolesAsync();
            return View(vm);
        }


        // ================== CẬP NHẬT ==================
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return HttpNotFound();

            ViewBag.Departments = await _service.GetDepartmentsAsync();
            ViewBag.Roles = await _service.GetRolesAsync(); // cần thêm hàm GetRolesAsync

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(vm);
                    TempData["Success"] = "Cập nhật thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // log ex nếu có logger
                    TempData["Error"] = ex.Message;
                }
            }

            ViewBag.Departments = await _service.GetDepartmentsAsync();
            ViewBag.Roles = await _service.GetRolesAsync();
            return View(vm);
        }


        // ================== XÓA ==================
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.SoftDeleteAsync(id);
            return RedirectToAction("Index");
        }

        // ================== VÔ HIỆU HÓA / KÍCH HOẠT ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleActive(int id)
        {
            try
            {
                int currentUserId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
                bool newState = await _service.ToggleActiveAsync(id); // trả về IsActive sau khi toggle

                if (currentUserId == id && newState == false)
                {
                    // Nếu user tự vô hiệu hóa mình -> logout ngay
                    Session.Clear();
                    try { FormsAuthentication.SignOut(); } catch { }
                    return RedirectToAction("Login", "Account", new { reason = "disabled" });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ================== CẬP NHẬT THÔNG TIN CÁ NHÂN ==================

        [HttpGet]
        public async Task<ActionResult> UserProfile()
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0) return RedirectToAction("Login", "Account");

            var vm = await _service.GetProfileAsync(userId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(UserProfileVM model, HttpPostedFileBase avatarFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0) return RedirectToAction("Login", "Account");

            model.UserId = userId;

            try
            {
                await _service.UpdateProfileAsync(model, avatarFile, Server.MapPath("~"));

                // Cập nhật session
                Session["FullName"] = model.FullName;
                if (!string.IsNullOrEmpty(model.AvatarUrl))
                    Session["UserAvatar"] = model.AvatarUrl;

                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }

            return RedirectToAction("UserProfile");
        }





    }
}
