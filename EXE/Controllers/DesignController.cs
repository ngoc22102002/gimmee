using Microsoft.AspNetCore.Mvc;

namespace EXE.Controllers
{
    public class DesignController : Controller
    {
        public IActionResult Index()
        {
            var userSessionID = HttpContext.Session.GetInt32("UserSessionID");
            if (userSessionID != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Login");
        }
    }
}
