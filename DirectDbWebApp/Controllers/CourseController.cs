using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class CourseController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
