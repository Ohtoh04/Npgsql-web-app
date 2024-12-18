using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.ViewModels;
using DirectDbWebApp.Models;
using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using Npgsql;
using DirectDbWebApp.Domain;

namespace DirectDbWebApp.Controllers {
    public class CourseController : Controller {
        private readonly HttpClient _httpClient;
        
        public CourseController() {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7179/");
        }

        [HttpGet("Courses")]
        public async Task<IActionResult> Courses() {
            try {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + "api/courses");

                if (response.IsSuccessStatusCode) {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    };

                    // Deserialize into List<ExpandoObject>
                    var coursesData = JsonSerializer.Deserialize<List<ExpandoObject>>(jsonString, options);
                    return View(coursesData);
                } else {
                    var errorModel = new ErrorViewModel {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    };
                    ModelState.AddModelError(string.Empty, "Unable to fetch courses. Please try again later.");
                    return View("Error", errorModel);
                }
            } catch (Exception ex) {
                var errorModel = new ErrorViewModel {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                };
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return View("Error", errorModel);
            }
        }


        //
        [HttpGet("course/{id}")]
        public async Task<IActionResult> Course(int id) {
            try {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/coursedata/{id}");

                if (response.IsSuccessStatusCode) {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    };

                    var courseData = JsonSerializer.Deserialize<Course>(jsonString, options);

                    //var courseTreeViewModel = MapToCourseTreeViewModel(courseData);
                    return View(courseData);
                } else {
                    var errorModel = new ErrorViewModel {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    };
                    ModelState.AddModelError(string.Empty, "Unable to fetch course data. Please try again later.");
                    return View("Error", errorModel);
                }
            } catch (Exception ex) {
                var errorModel = new ErrorViewModel {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                };
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return View("Error", errorModel);
            }
        }



        [HttpGet("mycourses")]
        public async Task<IActionResult> MyCourses() {
            try {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/courses?UserId={this.HttpContext.ToString}");// PLACEHOLDER

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

        //public async Task<IActionResult> Module() {
        //    var moduleResponse = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/courses/{id}");

        //    return PartialView();
        //}

        //public async Task<IActionResult> Lesson() {
        //    var lessonResponse = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/lessons/{id}");

        //    return PartialView();
        //}

        //private CourseTreeViewModel MapToCourseTreeViewModel(dynamic rawData) {
        //    // Extract the Course data
        //    var course = new {
        //        CourseId = rawData?.courseId,
        //        Title = rawData?.title,
        //        Description = rawData?.description,
        //        CourseType = rawData?.courseType,
        //        Price = rawData?.price,
        //        Duration = rawData?.duration,
        //        DateCreated = rawData?.dateCreated,
        //        Rating = rawData?.rating
        //    };

        //    // Extract the Modules
        //    var modules = new List<dynamic>();
        //    var units = new List<List<dynamic>>();

        //    foreach (var module in rawData.modules) {
        //        modules.Add(new {
        //            ModuleId = module?.moduleId,
        //            Title = module?.title,
        //            Description = module?.description,
        //            Sequence = module?.sequence
        //        });

        //        // Extract Units for each Module
        //        var moduleUnits = new List<dynamic>();
        //        if (module?.units != null) {
        //            foreach (var unit in module.units) {
        //                moduleUnits.Add(new {
        //                    UnitId = unit?.unitId,
        //                    Title = unit?.title,
        //                    Description = unit?.description,
        //                    Sequence = unit?.sequence
        //                });
        //            }
        //        }

        //        units.Add(moduleUnits);
        //    }
            

        //    // Return the ViewModel
        //    return new CourseTreeViewModel(course, modules, units);
        //}
    }
}
