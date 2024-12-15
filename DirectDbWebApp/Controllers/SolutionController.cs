using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class SolutionController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
