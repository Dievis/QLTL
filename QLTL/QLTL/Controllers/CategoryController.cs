using QLTL.Attributes;
using QLTL.Repositories;
using QLTL.Services;
using QLTL.ViewModels.CategoryVM;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class CategoryController : Controller
    {
        private readonly CategoryService _service;

        public CategoryController()
        {
            var db = new Models.QLTL_NEWEntities();
            var categoryRepo = new GenericRepository<Models.Category>(db);
            var typeRepo = new GenericRepository<Models.CategoryType>(db);
            var typeService = new CategoryTypeService(typeRepo);

            _service = new CategoryService(categoryRepo, typeService);
        }

        // ================== CATEGORY CRUD ==================
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
        public async Task<ActionResult> Create()
        {
            var vm = await _service.PrepareCreateVMAsync();
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(vm);
                return RedirectToAction("Index");
            }

            // POST fail thì cũng gọi service để refill dropdown
            vm = await _service.PrepareCreateVMAsync();
            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var vm = await _service.PrepareEditVMAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CategoryViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(vm);
                return RedirectToAction("Index");
            }

            vm = await _service.PrepareEditVMAsync(vm.CategoryId);
            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.SoftDeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
