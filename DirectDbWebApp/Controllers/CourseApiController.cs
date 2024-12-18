using DirectDbWebApp.Domain;
using DirectDbWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Reflection;
using System.Security.Claims;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api")]
    public class CourseApiController : ControllerBase {
        private readonly string _connectionString;

        public CourseApiController(IConfiguration Config) {
            this._connectionString = Config.GetValue<string>("ConnectionString") ?? "";
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
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

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
            var query = "SELECT * FROM Course WHERE course_id = @id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync();

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

        // Get courses with filter
        [HttpGet("courses/filtered")]
        public async Task<IActionResult> GetCoursesWithFilter([FromForm] string Title = "", [FromForm] double MaxPrice = 100000, [FromForm] double MinRating = 0,
                                                              [FromForm] string courseType = "", [FromForm] string category = "") {
            var query = @"SELECT * FROM Course c
                          LEFT JOIN CourseCategory cc ON c.course_id = cc.course_id
                          JOIN Category cat ON cc.category_id = cat.category_id
                          WHERE price < @MaxPrice AND rating > @MinRating AND title ILIKE @Title AND coursetype = @courseType AND cat.name = @category";

            var courses = new List<object>();

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaxPrice", MaxPrice);
                cmd.Parameters.AddWithValue("@MinRating", MinRating);
                cmd.Parameters.AddWithValue("@Title", $"%{Title}%");
                cmd.Parameters.AddWithValue("@courseType", courseType);
                cmd.Parameters.AddWithValue("@category", category);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync()) {
                    courses.Add(new {
                        CourseId = reader["course_id"],
                        Title = reader["title"],
                        Description = reader["description"],
                        CourseType = reader["coursetype"],
                        Price = reader["price"],
                        Duration = reader["duration"],
                        DateCreated = reader["date_created"],
                        Rating = reader["rating"]
                    });
                }

                return Ok(courses);
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Create a new course
        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] dynamic course) {
            var query = @"INSERT INTO Course (title, description, coursetype, price, duration, rating)
                          VALUES (@Title, @Description, @CourseType, @Price, @Duration, @Rating)
                          RETURNING course_id;";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", (string)course.title);
                cmd.Parameters.AddWithValue("@Description", (string)course.description);
                cmd.Parameters.AddWithValue("@CourseType", (string)course.coursetype);
                cmd.Parameters.AddWithValue("@Price", (double)course.price);
                cmd.Parameters.AddWithValue("@Duration", course.duration.ToString());
                cmd.Parameters.AddWithValue("@Rating", (double)course.rating);

                var courseId = await cmd.ExecuteScalarAsync();

                return Created($"/api/courses/{courseId}", new { CourseId = courseId });
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Update a course
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] dynamic course) {
            var query = @"UPDATE Course
                          SET title = @Title,
                              description = @Description,
                              coursetype = @CourseType,
                              price = @Price,
                              duration = @Duration,
                              rating = @Rating
                          WHERE course_id = @Id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", (string)course.title);
                cmd.Parameters.AddWithValue("@Description", (string)course.description);
                cmd.Parameters.AddWithValue("@CourseType", (string)course.coursetype);
                cmd.Parameters.AddWithValue("@Price", (double)course.price);
                cmd.Parameters.AddWithValue("@Duration", course.duration.ToString());
                cmd.Parameters.AddWithValue("@Rating", (double)course.rating);
                cmd.Parameters.AddWithValue("@Id", id);

                await cmd.ExecuteNonQueryAsync();

                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Delete a course
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id) {
            var query = "DELETE FROM Course WHERE course_id = @Id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                await cmd.ExecuteNonQueryAsync();

                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("coursedata/{id}")]
        public async Task<IActionResult> GetCourseData(int id) {
            var query = @"
                SELECT 
                    c.course_id AS CourseId,
                    c.title AS CourseTitle,
                    c.description AS CourseDescription,
                    c.coursetype AS CourseType,
                    c.price AS CoursePrice,
                    c.duration AS CourseDuration,
                    c.date_created AS CourseDateCreated,
                    c.rating AS CourseRating,
                    m.module_id AS ModuleId,
                    m.title AS ModuleTitle,
                    m.description AS ModuleDescription,
                    m.sequence AS ModuleSequence,
                    u.unit_id AS UnitId,
                    u.title AS UnitTitle,
                    u.description AS UnitDescription,
                    u.sequence AS UnitSequence
                FROM Course c
                LEFT JOIN Module m ON c.course_id = m.course_id
                LEFT JOIN Unit u ON m.module_id = u.module_id
                WHERE c.course_id = @courseId
                ORDER BY m.sequence, u.sequence;";

            var queryVerification = @"
                SELECT *
                FROM CourseUser
                WHERE course_id = @courseId AND user_id = @userId;";

            try {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId)) {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }

                Course courseData = null;
                var modulesDictionary = new Dictionary<int, CourseModule>();

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // Begin transaction
                await using var transaction = await conn.BeginTransactionAsync();

                try {
                    // Verify user-course relationship
                    await using var verificationCmd = new NpgsqlCommand(queryVerification, conn, transaction);
                    verificationCmd.Parameters.AddWithValue("@courseId", id);
                    verificationCmd.Parameters.AddWithValue("@userId", userId);

                    var hasAccess = false;
                    await using (var verificationReader = await verificationCmd.ExecuteReaderAsync()) {
                        hasAccess = await verificationReader.ReadAsync();
                    }

                    if (!hasAccess) {
                        await transaction.RollbackAsync();
                        return Forbid( "You do not have access to this course." );
                    }

                    // Fetch course data
                    await using var cmd = new NpgsqlCommand(query, conn, transaction);
                    cmd.Parameters.AddWithValue("@courseId", id);

                    await using (var reader = await cmd.ExecuteReaderAsync()) {
                        while (await reader.ReadAsync()) {
                            // Map the course data only once
                            if (courseData == null) {
                                courseData = new Course {
                                    CourseId = (int)reader["CourseId"],
                                    Title = reader["CourseTitle"].ToString(),
                                    Description = reader["CourseDescription"].ToString(),
                                    CourseType = reader["CourseType"].ToString(),
                                    Price = reader["CoursePrice"] == DBNull.Value ? (decimal?)null : (decimal)reader["CoursePrice"],
                                    Duration = reader["CourseDuration"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["CourseDuration"],
                                    DateCreated = (DateTime)reader["CourseDateCreated"],
                                    Rating = reader["CourseRating"] == DBNull.Value ? (decimal?)null : (decimal)reader["CourseRating"]
                                };
                            }

                            // Map modules
                            var moduleId = reader["ModuleId"] == DBNull.Value ? null : (int?)reader["ModuleId"];
                            if (moduleId.HasValue) {
                                if (!modulesDictionary.TryGetValue(moduleId.Value, out var module)) {
                                    module = new CourseModule {
                                        ModuleId = moduleId.Value,
                                        CourseId = courseData.CourseId,
                                        Title = reader["ModuleTitle"].ToString(),
                                        Description = reader["ModuleDescription"].ToString(),
                                        Sequence = reader["ModuleSequence"] == DBNull.Value ? (int?)null : (int)reader["ModuleSequence"]
                                    };
                                    modulesDictionary[moduleId.Value] = module;
                                    courseData.Modules.Add(module);
                                }

                                // Map units
                                var unitId = reader["UnitId"] == DBNull.Value ? null : (int?)reader["UnitId"];
                                if (unitId.HasValue) {
                                    module.Units.Add(new Unit {
                                        UnitId = unitId.Value,
                                        ModuleId = module.ModuleId,
                                        Title = reader["UnitTitle"].ToString(),
                                        Description = reader["UnitDescription"].ToString(),
                                        Sequence = reader["UnitSequence"] == DBNull.Value ? (int?)null : (int)reader["UnitSequence"]
                                    });
                                }
                            }
                        }
                    }

                    if (courseData == null) {
                        await transaction.RollbackAsync();
                        return NotFound(new { Message = "Course not found." });
                    }

                    // Commit transaction
                    await transaction.CommitAsync();
                    return Ok(courseData);

                } catch (Exception) {
                    // Rollback transaction in case of any error
                    await transaction.RollbackAsync();
                    throw;
                }

            } catch (Exception ex) {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

    }
}
