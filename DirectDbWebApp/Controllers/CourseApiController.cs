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

        [HttpGet("my-courses/{userId}")]
        public async Task<IActionResult> GetMyCourses(string userId) {
            var query = @"SELECT c.course_id, c.title, c.rating, c.coursetype, c.price, c.duration, c.date_created, cu.relation_type
              	        FROM Course c
              	        JOIN CourseUser cu ON c.course_id = cu.course_id
              	        WHERE cu.user_id = @UserId";

            var courses = new List<object>();

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                if(Int32.TryParse(userId, out int UserId)) 
                 cmd.Parameters.AddWithValue("@UserId", UserId);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync()) {
                    courses.Add(new {
                        CourseId = reader["course_id"],
                        Title = reader["title"],
                        Rating = reader["rating"],
                        CourseType = reader["coursetype"],
                        Price = reader["price"],
                        Duration = reader["duration"],
                        DateCreated = reader["date_created"],
                        RelationType = reader["relation_type"]
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
        public async Task<IActionResult> GetCoursesWithFilter([FromQuery] string Title = "", [FromQuery] double MaxPrice = 100000,
                                         [FromQuery] double MinRating = 0, [FromQuery] string courseType = "", [FromQuery] string category = "") {
            // Start with the base query
            var query = @"SELECT *,cat.name AS category
                          FROM Course c
              	          LEFT JOIN CourseCategory cc ON c.course_id = cc.course_id
              	          JOIN Category cat ON cc.category_id = cat.category_id
              	          WHERE price < @MaxPrice AND rating > @MinRating";

            // Create a dictionary to hold parameters
            var parameters = new Dictionary<string, object> {
                { "@MaxPrice", MaxPrice },
                { "@MinRating", MinRating/10.0 }
            };

            // Append filters dynamically
            if (!string.IsNullOrWhiteSpace(Title)) {
                query += " AND title ILIKE @Title";
                parameters.Add("@Title", $"%{Title}%");
            }

            if (!string.IsNullOrWhiteSpace(courseType)) {
                query += " AND coursetype = @courseType";
                parameters.Add("@courseType", courseType);
            }

            if (!string.IsNullOrWhiteSpace(category)) {
                query += " AND cat.name = @category";
                parameters.Add("@category", category);
            }

            var courses = new List<object>();

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);

                // Add parameters dynamically
                foreach (var param in parameters) {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

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
                        Rating = reader["rating"],
                        Category = reader["category"]
                    });
                }

                return Ok(courses);
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("courses/{userId}")]
        public async Task<IActionResult> CreateCourse([FromBody] Course course, string userId) {
            var insertCourseQuery = @"INSERT INTO Course (title, description, coursetype, price, duration, rating)
                              VALUES (@Title, @Description, @CourseType, @Price, @Duration, @Rating)
                              RETURNING course_id;";

            var verificationQuery = @"SELECT 1
                              FROM dbuser
                              WHERE (role = 'Admin' OR role = 'Creator') AND user_id = @UserId;";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var transaction = await conn.BeginTransactionAsync();

                try {
                    // Verify user role
                    await using (var verificationCmd = new NpgsqlCommand(verificationQuery, conn, transaction)) {
                        verificationCmd.Parameters.AddWithValue("@UserId", userId);

                        var verificationResult = await verificationCmd.ExecuteScalarAsync();
                        if (verificationResult == null) {
                            return Unauthorized(new { message = "User is not authorized to create courses." });
                        }
                    }

                    // Insert course
                    await using (var insertCmd = new NpgsqlCommand(insertCourseQuery, conn, transaction)) {
                        insertCmd.Parameters.AddWithValue("@Title", course.Title);
                        insertCmd.Parameters.AddWithValue("@Description", course.Description);
                        insertCmd.Parameters.AddWithValue("@CourseType", course.CourseType);
                        insertCmd.Parameters.AddWithValue("@Price", (object?)course.Price ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Duration", (object?)course.Duration ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Rating", (object?)course.Rating ?? DBNull.Value);

                        var courseId = await insertCmd.ExecuteScalarAsync();

                        // Commit transaction
                        await transaction.CommitAsync();

                        return Created($"/api/courses/{courseId}", new { CourseId = courseId });
                    }
                } catch {
                    // Rollback transaction in case of an error
                    await transaction.RollbackAsync();
                    throw;
                }
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course course) {
            // Ensure that the course ID from the route matches the course ID in the body
            if (id != course.CourseId) {
                return BadRequest("Course ID mismatch.");
            }

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
                cmd.Parameters.AddWithValue("@Title", course.Title);
                cmd.Parameters.AddWithValue("@Description", course.Description);
                cmd.Parameters.AddWithValue("@CourseType", course.CourseType);
                cmd.Parameters.AddWithValue("@Price", course.Price.HasValue ? (object)course.Price.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Duration", course.Duration.HasValue ? (object)course.Duration.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Rating", course.Rating.HasValue ? (object)course.Rating.Value : DBNull.Value);
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
                    u.sequence AS UnitSequence,
                    l.lesson_id AS LessonId,
                    l.title AS LEssonTitle,
                    l.content AS LessonContent,
                    l.sequence AS LessonSequence,
                FROM Course c
                LEFT JOIN Module m ON c.course_id = m.course_id
                LEFT JOIN Unit u ON m.module_id = u.module_id
                LEFT JOIN Lesson l ON u.unit_id = l.unit_id
                WHERE c.course_id = @courseId
                ORDER BY m.sequence, u.sequence, l.sequence;";

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
                var unitsDictionary = new Dictionary<int, Unit>();

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
                                    if (!unitsDictionary.TryGetValue(unitId.Value, out var unit)) {
                                        unit = new Unit {
                                            UnitId = unitId.Value,
                                            ModuleId = module.ModuleId,
                                            Title = reader["UnitTitle"].ToString(),
                                            Description = reader["UnitDescription"].ToString(),
                                            Sequence = reader["UnitSequence"] == DBNull.Value ? (int?)null : (int)reader["UnitSequence"],
                                            Lessons = new List<Lesson>() // Initialize Lessons list
                                        };
                                        unitsDictionary[unitId.Value] = unit;
                                        module.Units.Add(unit); // Add unit to its module
                                    }

                                    // Map lessons
                                    var lessonId = reader["LessonId"] == DBNull.Value ? null : (int?)reader["LessonId"];
                                    if (lessonId.HasValue) {
                                        unit.Lessons.Add(new Lesson {
                                            LessonId = lessonId.Value,
                                            UnitId = unit.UnitId,
                                            Title = reader["LessonTitle"].ToString(),
                                            Content = reader["LessonContent"].ToString(),
                                            Sequence = reader["LessonSequence"] == DBNull.Value ? (int?)null : (int)reader["LessonSequence"],
                                        });
                                    }
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
