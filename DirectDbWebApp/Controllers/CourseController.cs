using Microsoft.AspNetCore.Mvc;

namespace DirectDbWebApp.Controllers {
    public class CourseController : Controller {
        private readonly HttpClient _httpClient = new HttpClient();

        //public CourseController() { 
        //}

        public async Task<IActionResult> Courses() {
            try {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + "api/courses");

                if (response.IsSuccessStatusCode) {
                    var coursesData = await response.Content.ReadFromJsonAsync<List<dynamic>>();

                    return View(coursesData);
                } else {
                    ModelState.AddModelError(string.Empty, "Unable to fetch courses. Please try again later.");
                    return View("Error"); 
                }
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return View("Error");
            }
        }


        public async Task<IActionResult> MyCourses() {
            try {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/courses?client_id={this.HttpContext.ToString}");// PLACEHOLDER

                if (response.IsSuccessStatusCode) {
                    var coursesData = await response.Content.ReadFromJsonAsync<List<dynamic>>();

                    return View(coursesData);
                } else {
                    ModelState.AddModelError(string.Empty, "Unable to fetch courses. Please try again later.");
                    return View("Error");
                }
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return View("Error");
            }
        }
    }
}
