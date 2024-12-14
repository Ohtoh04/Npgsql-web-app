using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class UserController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
