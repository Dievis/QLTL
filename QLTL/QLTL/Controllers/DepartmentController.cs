using QLTL.Attributes;
using QLTL.Repositories;
using QLTL.Services;
using QLTL.ViewModels.DepartmentVM;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class DepartmentController : Controller
    {
        private readonly DepartmentService _service;

        public DepartmentController()
        {
            var db = new Models.QLTL_NEWEntities();
            var repo = new GenericRepository<Models.Department>(db);
            _service = new DepartmentService(repo);
        }

        // ================== DEPARTMENT CRUD ==================
        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var vm = await _service.GetAllAsync(search, isDeleted, page, pageSize);
            return View(vm);
        }

        public async Task<ActionResult> Details(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpGet]
        [AuthorizeCustom(Permissions = "Depart.Create")]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DepartmentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        [AuthorizeCustom(Permissions ="Depart.Edit")]

        public async Task<ActionResult> Edit(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpPost]
        [AuthorizeCustom(Permissions = "Depart.Delete")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.SoftDeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
