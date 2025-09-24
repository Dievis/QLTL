using QLTL.Attributes;
using QLTL.Models;
using QLTL.Repositories;
using QLTL.Services;
using QLTL.ViewModels.PermissionVM;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class PermissionController : Controller
    {
        private readonly PermissionService _permissionService;

        public PermissionController()
        {
            var db = new QLTL_NEWEntities();
            var permRepo = new GenericRepository<Permission>(db);
            _permissionService = new PermissionService(permRepo);
        }

        // ================== PERMISSION CRUD ==================

        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var vm = await _permissionService.GetAllPermissionsAsync(search, isDeleted, page, pageSize);
            return View(vm);
        }

        public async Task<ActionResult> Details(int id)
        {
            var perm = await _permissionService.GetPermissionByIdAsync(id);
            if (perm == null) return HttpNotFound();
            return View(perm);
        }

        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PermissionViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _permissionService.CreatePermissionAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var perm = await _permissionService.GetPermissionByIdAsync(id);
            if (perm == null) return HttpNotFound();
            return View(perm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PermissionViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _permissionService.UpdatePermissionAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            await _permissionService.SoftDeletePermissionAsync(id);
            return RedirectToAction("Index");
        }
    }
}
