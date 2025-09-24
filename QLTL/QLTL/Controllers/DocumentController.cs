using QLTL.Attributes;
using QLTL.Services;
using QLTL.ViewModels.DocumentVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class DocumentController : Controller
    {
        private readonly DocumentService _service;

        public DocumentController()
        {
            var db = new Models.QLTL_NEWEntities();
            var repo = new Repositories.GenericRepository<Models.Document>(db);
            var favRepo = new Repositories.GenericRepository<Models.FavoriteDocument>(db);
            var docDeptRepo = new Repositories.GenericRepository<Models.DocumentDepartment>(db);
            var approvalRepo = new Repositories.GenericRepository<Models.DocumentApproval>(db);
            var changeLogRepo = new Repositories.GenericRepository<Models.DocumentChangeLog>(db);
            var userRepo = new Repositories.GenericRepository<Models.User>(db);
            var categoryRepo = new Repositories.GenericRepository<Models.Category>(db);
            var docTypeRepo = new Repositories.GenericRepository<Models.DocumentType>(db);
            var departmentRepo = new Repositories.GenericRepository<Models.Department>(db);


            _service = new DocumentService(repo, favRepo, docDeptRepo, approvalRepo, changeLogRepo, userRepo, categoryRepo, docTypeRepo, departmentRepo);
        }

        // ========== LIST ==========
        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1)
        {
            int pageSize = 10;
            var model = await _service.GetAllAsync(search, isDeleted, page, pageSize);
            return View(model);
        }


        // ========== DETAIL ==========
        public async Task<ActionResult> Details(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            if (doc == null) return HttpNotFound();

            return View(doc);
        }

        // ========== CREATE ==========
        [HttpGet]
        [AuthorizeCustom(Permissions = "Document.Create")]
        public async Task<ActionResult> Create()
        {
            await LoadDropdownsAsync(); // <-- nạp dropdown trước khi trả view
            return View(new DocumentCreateVM());
        }

        [HttpPost]
        public async Task<ActionResult> Create(DocumentCreateVM model, HttpPostedFileBase file, List<int> departmentIds)
        {
            if (ModelState.IsValid)
            {
                if (Session["UserId"] == null)
                {
                    ModelState.AddModelError("", "Phiên đăng nhập đã hết hạn.");
                    return View(model);
                }

                int userId = (int)Session["UserId"]; // lấy user hiện tại
                model.UploaderID = userId; // gán userId vào ViewModel để CreateAsync dùng

                var error = await _service.CreateAsync(model, file, Server.MapPath("~"), departmentIds);
                if (error == null)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", error);
            }

            await LoadDropdownsAsync(); // nạp dropdown nếu return View
            return View(model);
        }


        // ========== EDIT ==========
        [HttpGet]
        [AuthorizeCustom(Permissions = "Document.Edit")]

        public async Task<ActionResult> Edit(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            if (doc == null) return HttpNotFound();

            var vm = new DocumentEditVM
            {
                DocumentId = doc.DocumentId,
                Title = doc.Title,
                Content = doc.Content,
                CategoryId = doc.CategoryId,
                DocumentTypeId = doc.DocumentTypeId,
                FilePath = doc.FilePath,
                UploaderID = doc.UploaderID,
                DepartmentIds = doc.Departments.Select(d => d.DepartmentId).ToList() // đảm bảo có danh sách department
            };
           await LoadDropdownsAsync(); // <--- phải gọi trước khi trả view

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(DocumentEditVM model, HttpPostedFileBase file, List<int> departmentIds)
        {
            if (ModelState.IsValid)
            {
                if (Session["UserId"] == null)
                {
                    ModelState.AddModelError("", "Phiên đăng nhập đã hết hạn.");
                    return View(model);
                }

                int userId = (int)Session["UserId"]; // lấy user hiện tại
                model.UploaderID = userId; // hoặc gán vào tham số UpdateAsync

                var error = await _service.UpdateAsync(model, file, Server.MapPath("~"), departmentIds);
                if (error == null)
                    return RedirectToAction("Index");
                ModelState.AddModelError("", error);
            }

            await LoadDropdownsAsync(); // nạp dropdown nếu return View
            return View(model);
        }


        // ========== DELETE (Soft Delete) ==========
        [HttpPost]
        [AuthorizeCustom(Permissions = "Document.Delete")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.SoftDeleteDocumentAsync(id);
            return RedirectToAction("Index");
        }

        // ========== FAVORITE ==========
        // List yêu thích
        [HttpGet]
        public async Task<ActionResult> MyFavorites(string search, int page = 1)
        {
            int pageSize = 10;

            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account"); // Hoặc xử lý tùy app

            int userId = (int)Session["UserId"]; // Lấy userId từ session

            var model = await _service.GetFavoritesAsync(userId, search, page, pageSize);
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> AddFavorite(int documentId, int userId)
        {
            string error = await _service.AddFavoriteAsync(documentId, userId);
            if (!string.IsNullOrEmpty(error))
                return Json(new { success = false, message = error });

            return Json(new { success = true });
        }

        // Xóa yêu thích
        [HttpPost]
        public async Task<ActionResult> RemoveFavorite(int documentId, int userId)
        {
            string error = await _service.RemoveFavoriteAsync(documentId, userId);
            if (!string.IsNullOrEmpty(error))
                return Json(new { success = false, message = error });

            return Json(new { success = true });
        }

        // Kiểm tra trạng thái favorite
        [HttpGet]
        public async Task<ActionResult> IsFavorite(int documentId, int userId)
        {
            bool isFav = await _service.IsFavoriteAsync(documentId, userId);
            return Json(new { success = true, isFavorite = isFav }, JsonRequestBehavior.AllowGet);
        }


        // ========== DOWNLOAD ==========
        public async Task<ActionResult> Download(int id)
        {
            string rootPath = Server.MapPath("~");
            var fileData = await _service.GetFileAsync(id, rootPath);

            if (fileData == null)
                return HttpNotFound("File không tồn tại");

            if (fileData.Value.Inline)
            {
                return File(fileData.Value.FileBytes, fileData.Value.ContentType);
            }
            else
            {
                return File(fileData.Value.FileBytes, fileData.Value.ContentType, fileData.Value.FileName);
            }
        }


        // ========== APPROVAL ==========
        [HttpPost]
        public async Task<ActionResult> Approve(int approvalId, int approverId, string reason = null)
        {
            var error = await _service.ReviewApprovalAsync(approvalId, approverId, "Approved", reason);
            if (error != null) TempData["Error"] = error;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Reject(int approvalId, int approverId, string reason = null)
        {
            var error = await _service.ReviewApprovalAsync(approvalId, approverId, "Rejected", reason);
            if (error != null) TempData["Error"] = error;
            return RedirectToAction("Index");
        }

        // ================== HÀM HỖ TRỢ ==================
        private async Task LoadDropdownsAsync()
        {
            // Lấy tất cả category
            var categories = (await _service.GetAllCategoriesAsync())?.ToList() ?? new List<Models.Category>();
            ViewBag.Categories = categories;

            // Lấy tất cả document type
            var docTypes = (await _service.GetAllDocumentTypesAsync())?.ToList() ?? new List<Models.DocumentType>();
            ViewBag.DocumentTypes = docTypes;

            // Lấy tất cả department
            var departments = (await _service.GetAllDepartmentsAsync())?.ToList() ?? new List<Models.Department>();
            ViewBag.Departments = departments;
        }



    }
}
