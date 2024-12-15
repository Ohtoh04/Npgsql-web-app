using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class SubscriptionController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
