﻿using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.ViewModels;
using DirectDbWebApp.Models;
using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using Npgsql;
using DirectDbWebApp.Domain;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
                var responseCategories = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/category");

                if (response.IsSuccessStatusCode) {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    };

                    // Deserialize into List<ExpandoObject>
                    var coursesData = JsonSerializer.Deserialize<List<ExpandoObject>>(jsonString, options);

                    if (responseCategories.IsSuccessStatusCode) {
                        ViewBag.Categories = JsonSerializer.Deserialize<List<Category>>(await responseCategories.Content.ReadAsStringAsync());
                    }
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


        [HttpPost("CoursesFiltered")]
        public async Task<IActionResult> CoursesFiltered(string title = "", double maxPrice = 100000, [FromForm(Name = "minRating")] string minRatingInput = "0", string type = "", string category = "") {
            try {
                double minRating = 0; 
                if (!string.IsNullOrEmpty(minRatingInput) && double.TryParse(minRatingInput, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedMinRating))
                    { minRating = parsedMinRating; }

                // Build the query string
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(title)) queryParams.Add($"Title={Uri.EscapeDataString(title)}");
                if (maxPrice != 100000) queryParams.Add($"MaxPrice={maxPrice}");
                if (minRating != 0) queryParams.Add($"MinRating={minRating}");
                if (!string.IsNullOrEmpty(type)) queryParams.Add($"courseType={Uri.EscapeDataString(type)}");
                if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");

                var queryString = string.Join("&", queryParams);

                // Make the GET request
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress.AbsoluteUri}api/courses/filtered?{queryString}");
                var responseCategories = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/category");

                if (response.IsSuccessStatusCode) {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    };

                    var coursesData = JsonSerializer.Deserialize<List<ExpandoObject>>(jsonString, options);

                    if (responseCategories.IsSuccessStatusCode) {
                        ViewBag.Categories = JsonSerializer.Deserialize<List<Category>>(await responseCategories.Content.ReadAsStringAsync());
                    }

                    return View("Courses",coursesData);
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

                    //if (responseCategories.IsSuccessStatusCode) {
                    //    ViewBag.Categories = JsonSerializer.Deserialize<List<Category>>(await responseCategories.Content.ReadAsStringAsync());
                    //}
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


        [HttpGet("unitbar-partial/{unitId}")]
        public async Task<IActionResult> UnitBarPartial(int unitId) {
            try {
                var lessonsResponse = await _httpClient.GetAsync($"{_httpClient.BaseAddress.AbsoluteUri}api/lessons/{}")
            } catch (Exception ex) {

            }
            return PartialView("_UnitBarPartial");
        }


        [HttpGet("mycourses")]
        public async Task<IActionResult> MyCourses() {
            try {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/my-courses/{User.FindFirstValue(ClaimTypes.NameIdentifier)}");
                if (response.IsSuccessStatusCode) {
                    var coursesData = await response.Content.ReadFromJsonAsync<List<dynamic>>();

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
