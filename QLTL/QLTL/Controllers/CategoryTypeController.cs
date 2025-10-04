using QLTL.Attributes;
using QLTL.Repositories;
using QLTL.Services;
using QLTL.ViewModels.CategoryTypeVM;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class CategoryTypeController : Controller
    {
        private readonly CategoryTypeService _service;

        public CategoryTypeController()
        {
            var db = new Models.QLTL_NEWEntities();
            var repo = new GenericRepository<Models.CategoryType>(db);
            _service = new CategoryTypeService(repo);
        }

        // ================== CATEGORYTYPE CRUD ==================
        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var vm = await _service.GetAllAsync(search, isDeleted, page, pageSize);
            return View(vm);
        }

        [AuthorizeCustom(Permissions = "CategoryType.View, View", RequireAll = false)]
        public async Task<ActionResult> Details(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpGet]
        [AuthorizeCustom(Permissions = "CategoryType.Create")]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryTypeViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        [AuthorizeCustom(Permissions = "CategoryType.Edit")]
        public async Task<ActionResult> Edit(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CategoryTypeViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpPost]
        [AuthorizeCustom(Permissions = "CategoryType.Delete")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.SoftDeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
