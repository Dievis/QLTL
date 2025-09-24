using QLTL.Attributes;
using QLTL.Services;
using QLTL.ViewModels.RoleVM;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class RoleController : Controller
    {
        private readonly RoleService _roleService;
        private readonly PermissionService _permissionService;

        public RoleController()
        {
            var db = new Models.QLTL_NEWEntities();
            var roleRepo = new Repositories.GenericRepository<Models.Role>(db);
            var permRepo = new Repositories.GenericRepository<Models.Permission>(db);
            var rolePermRepo = new Repositories.GenericRepository<Models.RolePermission>(db);

            _roleService = new RoleService(roleRepo, permRepo, rolePermRepo);
            _permissionService = new PermissionService(permRepo);
        }

        // ================== ROLE CRUD ==================

        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var vm = await _roleService.GetAllRolesAsync(search, isDeleted, page, pageSize);
            return View(vm);
        }


        public async Task<ActionResult> Details(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return HttpNotFound();
            return View(role);
        }

        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoleViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _roleService.CreateRoleAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return HttpNotFound();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RoleViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _roleService.UpdateRoleAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            await _roleService.SoftDeleteRoleAsync(id);
            return RedirectToAction("Index");
        }

        // ================== ROLE PERMISSION ==================

        [HttpGet]
        public async Task<ActionResult> ManagePermissions(int roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null) return HttpNotFound();

            var allPermissions = await _permissionService.GetAllPermissionsNoPagingAsync();
            var rolePermissions = await _roleService.GetPermissionsByRoleAsync(roleId);

            var vm = new RolePermissionManageVM
            {
                RoleId = roleId,
                RoleName = role.RoleName,
                Permissions = allPermissions.Select(p => new RolePermissionCheckboxVM
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    IsAssigned = rolePermissions.Any(rp => rp.PermissionId == p.PermissionId)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManagePermissions(RolePermissionManageVM vm)
        {
            if (ModelState.IsValid)
            {
                var selectedPermissionIds = vm.Permissions
                                              .Where(p => p.IsAssigned)
                                              .Select(p => p.PermissionId)
                                              .ToList();

                await _roleService.AssignPermissionsToRoleAsync(vm.RoleId, selectedPermissionIds);
                return RedirectToAction("Index");
            }

            return View(vm);
        }
    }
}
