using DirectDbWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Reflection;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CourseApiController : ControllerBase {
        private readonly DataService _dataService;

        public CourseApiController(DataService dataService) {
            _dataService = dataService;
        }

        // Get all courses
        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses() {
            var query = @"SELECT c.course_id, c.title, c.rating, cat.name AS category
                           FROM Course c
                           JOIN CourseCategory cc ON c.course_id = cc.course_id
                           JOIN Category cat ON cc.category_id = cat.category_id";
            var courses = new List<object>();

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                while (await reader.ReadAsync()) {
                    courses.Add(new {
                        CourseId = reader["course_id"],
                        Title = reader["title"],
                        Rating = reader["rating"],
                        Category = reader["category"]
                    });
                }

                return Ok(courses);
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get course by ID
        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourseById(int id) {
            var query = $"SELECT * FROM Course WHERE course_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                if (await reader.ReadAsync()) {
                    var course = new {
                        CourseId = reader["course_id"],
                        Title = reader["title"],
                        Description = reader["description"],
                        CourseType = reader["coursetype"],
                        Price = reader["price"],
                        Duration = reader["duration"],
                        DateCreated = reader["date_created"],
                        Rating = reader["rating"]
                    };
                    return Ok(course);
                }

                return NotFound(new { message = "Course not found" });
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("courses/filtered")]
        public async Task<IActionResult> GetCoursesWithFilter([FromQuery] string Title = "", [FromQuery] double MaxPrice = 100000, [FromQuery] double MinRating = 0) {
            var query = $"SELECT * FROM Course WHERE price<{MaxPrice} AND rating>{MinRating} AND title ILIKE '%{Title}%';";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                if (await reader.ReadAsync()) {
                    var course = new {
                        CourseId = reader["course_id"],
                        Title = reader["title"],
                        Description = reader["description"],
                        CourseType = reader["coursetype"],
                        Price = reader["price"],
                        Duration = reader["duration"],
                        DateCreated = reader["date_created"],
                        Rating = reader["rating"]
                    };
                    return Ok(course);
                }

                return NotFound(new { message = "Course not found" });
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("courses/user")]
        public async Task<IActionResult> GetCoursesForUser([FromQuery] int UserId) {
            var query = $"SELECT c.course_id, c.title, c.description, c.coursetype, c.price, c.duration, c.date_created, c.rating, cu.relation_type, cu.date_joined" +
                        $"FROM Course c JOIN CourseUser cu ON c.course_id = cu.course_id WHERE cu.user_id = $1;";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                if (await reader.ReadAsync()) {
                    var course = new {
                        CourseId = reader["course_id"],
                        Title = reader["title"],
                        Description = reader["description"],
                        CourseType = reader["coursetype"],
                        Price = reader["price"],
                        Duration = reader["duration"],
                        DateCreated = reader["date_created"],
                        Rating = reader["rating"]
                    };
                    return Ok(course);
                }

                return NotFound(new { message = "Course not found" });
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        // Create a new course
        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] dynamic course) {
            var query = $@"
                INSERT INTO Course (title, description, coursetype, price, duration, rating)
                VALUES ('{course.title}', '{course.description}', '{course.coursetype}', {course.price}, INTERVAL '{course.duration}', {course.rating})
                RETURNING course_id;";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                if (await reader.ReadAsync()) {
                    return Created($"/api/courses/{reader["course_id"]}", new { CourseId = reader["course_id"] });
                }

                return BadRequest(new { message = "Failed to create course" });
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Update a course
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] dynamic course) {
            var query = $@"
                UPDATE Course
                SET title = '{course.title}',
                    description = '{course.description}',
                    coursetype = '{course.coursetype}',
                    price = {course.price},
                    duration = INTERVAL '{course.duration}',
                    rating = {course.rating}
                WHERE course_id = {id};";

            try {
                await _dataService.ExecuteQuery(query);
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Delete a course
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id) {
            var query = $"DELETE FROM Course WHERE course_id = {id}";

            try {
                await _dataService.ExecuteQuery(query);
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
