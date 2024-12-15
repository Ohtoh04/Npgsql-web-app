using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class PaymentController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
