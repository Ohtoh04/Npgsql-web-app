using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;
using DirectDbWebApp.Domain;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class LessonApiController : ControllerBase {
        private readonly string _connectionString;

        public LessonApiController(IConfiguration config) {
            _connectionString = config.GetValue<string>("ConnectionString") ?? throw new ArgumentNullException("ConnectionString is not configured.");
        }

        // GET: api/Lesson
        [HttpGet]
        public async Task<IActionResult> GetLessons() {
            var query = "SELECT lesson_id, unit_id, title, content, correct_solution, sequence FROM Lesson";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                var lessons = new List<Lesson>();

                while (await reader.ReadAsync()) {
                    lessons.Add(new Lesson {
                        LessonId = reader.GetInt32(0),
                        UnitId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Content = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CorrectSolution = reader.IsDBNull(4) ? default : JsonDocument.Parse(reader.GetString(4)).RootElement,
                        Sequence = reader.IsDBNull(5) ? default : reader.GetInt32(5)
                    });
                }

                return Ok(lessons);
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching lessons.", Details = ex.Message });
            }
        }

        // GET: api/Lesson/unit/{unitId}
        [HttpGet("unit/{unitId}")]
        public async Task<IActionResult> GetLessonsForUnit(int unitId) {
            var query = "SELECT lesson_id, title, content FROM Lesson WHERE unit_id = @unitId ORDER BY sequence";
            
            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("unitId", unitId);

                await using var reader = await cmd.ExecuteReaderAsync();

                var lessons = new List<Lesson>();

                while (await reader.ReadAsync()) {
                    lessons.Add(new Lesson {
                        LessonId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Content = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });
                }

                return Ok(lessons);
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching lessons.", Details = ex.Message });
            }
        }

        // GET: api/Lesson/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id) {
            var query = "SELECT lesson_id, unit_id, title, content, correct_solution, sequence FROM Lesson WHERE lesson_id = @id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", id);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync()) {
                    var lesson = new Lesson {
                        LessonId = reader.GetInt32(0),
                        UnitId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Content = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CorrectSolution = reader.IsDBNull(4) ? default : JsonDocument.Parse(reader.GetString(4)).RootElement,
                        Sequence = reader.IsDBNull(5) ? default : reader.GetInt32(5)
                    };

                    return Ok(lesson);
                }

                return NotFound(new { Message = "Lesson not found." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching the lesson.", Details = ex.Message });
            }
        }

        // POST: api/Lesson
        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] Lesson lesson) {
            if (lesson == null || lesson.UnitId == 0 || string.IsNullOrWhiteSpace(lesson.Title)) {
                return BadRequest(new { Message = "Invalid input. 'UnitId' and 'Title' are required." });
            }

            var query = "INSERT INTO Lesson (unit_id, title, content, correct_solution, sequence) " +
                        "VALUES (@unitId, @title, @content, @correctSolution, @sequence) RETURNING lesson_id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("unitId", lesson.UnitId);
                cmd.Parameters.AddWithValue("title", lesson.Title);
                cmd.Parameters.AddWithValue("content", (object?)lesson.Content ?? DBNull.Value);
                cmd.Parameters.AddWithValue("correctSolution", lesson.CorrectSolution.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("sequence", lesson.Sequence);

                var lessonId = await cmd.ExecuteScalarAsync();

                if (lessonId != null) {
                    return CreatedAtAction(nameof(GetLessonById), new { id = (int)lessonId }, lesson);
                }

                return StatusCode(500, new { Message = "An error occurred while creating the lesson." });
            } catch (PostgresException ex) when (ex.SqlState == "23503") // Foreign key violation
              {
                return BadRequest(new { Message = "The specified UnitId does not exist." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the lesson.", Details = ex.Message });
            }
        }

        // DELETE: api/Lesson/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id) {
            var query = "DELETE FROM Lesson WHERE lesson_id = @id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0) {
                    return NoContent();
                }

                return NotFound(new { Message = "Lesson not found." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the lesson.", Details = ex.Message });
            }
        }
    }

}
