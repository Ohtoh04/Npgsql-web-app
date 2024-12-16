﻿using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Extensions;
using System.Reflection.Metadata.Ecma335;
using DirectDbWebApp.ViewModels;

namespace DirectDbWebApp.Controllers {
    public class CourseController : Controller {
        private readonly HttpClient _httpClient = new HttpClient();

        //public CourseController() { 
        //}

        [HttpGet("Courses")]
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

        [HttpGet("Course/{id}")]
        public async Task<IActionResult> Course(int id) {
            try {

                var courseResponse = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/courses/{id}");
                var moduleResponse = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/modules/{id}");
                var unitResponse = await _httpClient.GetAsync(_httpClient.BaseAddress.AbsoluteUri + $"api/units/{id}");

                if (courseResponse.IsSuccessStatusCode) {
                    var coursesData = await courseResponse.Content.ReadFromJsonAsync<dynamic>();
                    var moduleData = await moduleResponse.Content.ReadFromJsonAsync<List<dynamic>>();
                    var unitData = await moduleResponse.Content.ReadFromJsonAsync<List<List<dynamic>>>();

                    return View(new CourseTreeViewModel(coursesData, moduleData, unitData));
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

        [HttpGet("MyCourses")]
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
    }
}
