using QLTL.Attributes;
using QLTL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QLTL.Controllers
{
    [AuthorizeCustom]
    public class HomeController : Controller
    {
        private readonly DocumentService _service;

        public HomeController()
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
        public async Task<ActionResult> Index(string search, bool? isDeleted, int page = 1, int pageSize = 10)
        {
            var model = await _service.GetApprovedDocumentsAsync(search, page, pageSize);
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}