using EXE.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace EXE.Controllers
{
    public class Demo3DController : Controller
    {
        private readonly Exe201Context _exeContext;
        public Demo3DController(Exe201Context exeContext)
        {
            exeContext = _exeContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(int id) { 
            if (id == 0)
            {
                return RedirectToAction("Index", "Cart");
            }
            var checkProject = _exeContext.Projects.FirstOrDefault(p => p.ProjectID == id);
            if (checkProject == null)
            {
                return RedirectToAction("Index", "Cart");
            }
            HttpContext.Session.SetString("imageFrontPath", checkProject.ImageFront);
            HttpContext.Session.SetString("imageBackPath", checkProject.ImageBack);

            return View();
        }
    }
}
