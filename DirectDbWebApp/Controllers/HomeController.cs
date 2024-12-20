using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using DirectDbWebApp.Domain;
using DirectDbWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HttpClient _httpClient = new HttpClient();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://localhost:7179/");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AddPayment() {
            return View();
        }

        public async Task<IActionResult> Profile() {
            try {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/user/{userId}");

                if (response.IsSuccessStatusCode) {
                    var userData = await response.Content.ReadFromJsonAsync<DbUser>();
                    return View(userData);
                } else {
                    var errorModel = new ErrorViewModel {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    };
                    ModelState.AddModelError(string.Empty, "Unable to fetch user. Please try again later.");
                    return View("Error");
                }
            } catch (Exception ex) {
                var errorModel = new ErrorViewModel {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                };
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return View("Error");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
