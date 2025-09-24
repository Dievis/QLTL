using QLTL.Attributes;
using QLTL.Services;
using QLTL.ViewModels.DocumentTypeVM;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class DocumentTypeController : Controller
    {
        private readonly DocumentTypeService _service;

        public DocumentTypeController()
        {
            var db = new Models.QLTL_NEWEntities();
            var repo = new Repositories.GenericRepository<Models.DocumentType>(db);
            _service = new DocumentTypeService(repo);
        }

        // ================== LIST (INDEX) ==================
        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var vm = await _service.GetAllDocumentTypesAsync(search, isDeleted, page, pageSize);
            return View(vm);
        }

        // ================== CREATE ==================
        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DocumentTypeViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateDocumentTypeAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        // ================== EDIT ==================
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var vm = await _service.GetDocumentTypeByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DocumentTypeViewModel vm)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateDocumentTypeAsync(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        // ================== DETAILS ==================
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var vm = await _service.GetDocumentTypeByIdAsync(id);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        // ================== SOFT DELETE ==================
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.SoftDeleteDocumentTypeAsync(id);
            return RedirectToAction("Index");
        }
    }
}
