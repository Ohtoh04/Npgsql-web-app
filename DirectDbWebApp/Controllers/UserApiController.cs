using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class UserApiController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
