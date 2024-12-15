using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class LessonApiController : ControllerBase {
        private readonly DataService _dataService;

        public LessonApiController(DataService dataService) {
            _dataService = dataService;
        }

        // GET: api/Lesson
        [HttpGet]
        public async Task<IActionResult> GetLessons() {
            var query = "SELECT lesson_id, unit_id, title, content, correct_solution, sequence FROM Lesson";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                var lessons = new List<object>();

                while (await reader.ReadAsync()) {
                    lessons.Add(new {
                        LessonId = reader.GetInt32(0),
                        UnitId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Content = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CorrectSolution = reader.IsDBNull(4) ? new JsonElement() : JsonDocument.Parse(reader.GetString(4)).RootElement,
                        Sequence = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
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
            var query = $"SELECT lesson_id, unit_id, title, content, correct_solution, sequence FROM Lesson WHERE lesson_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var lesson = new {
                        LessonId = reader.GetInt32(0),
                        UnitId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Content = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CorrectSolution = reader.IsDBNull(4) ? new JsonElement() : JsonDocument.Parse(reader.GetString(4)).RootElement,
                        Sequence = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
                    };

                    return Ok(lesson);
                } else {
                    return NotFound(new { Message = "Lesson not found." });
                }
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching the lesson.", Details = ex.Message });
            }
        }

        // POST: api/Lesson
        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] JsonElement body) {
            if (!body.TryGetProperty("unit_id", out JsonElement unitIdElement) || !body.TryGetProperty("title", out JsonElement titleElement)) {
                return BadRequest(new { Message = "Invalid input. 'unit_id' and 'title' are required." });
            }

            var unitId = unitIdElement.GetInt32();
            var title = titleElement.GetString();
            var content = body.TryGetProperty("content", out JsonElement contentElement) ? contentElement.GetString() : null;
            var correctSolution = body.TryGetProperty("correct_solution", out JsonElement correctSolutionElement) ? correctSolutionElement.ToString() : null;
            var sequence = body.TryGetProperty("sequence", out JsonElement sequenceElement) ? sequenceElement.GetInt32() : (int?)null;

            var query = "INSERT INTO Lesson (unit_id, title, content, correct_solution, sequence) " +
                        $"VALUES ({unitId}, '{title}', '{content}', '{correctSolution}', {sequence}) RETURNING lesson_id";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var lessonId = reader.GetInt32(0);
                    return CreatedAtAction(nameof(GetLessonById), new { id = lessonId }, new { LessonId = lessonId, UnitId = unitId, Title = title, Content = content, CorrectSolution = correctSolution, Sequence = sequence });
                }

                return StatusCode(500, new { Message = "An error occurred while creating the lesson." });
            } catch (PostgresException ex) when (ex.SqlState == "23503") // Foreign key violation
              {
                return BadRequest(new { Message = "The specified unit_id does not exist." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the lesson.", Details = ex.Message });
            }
        }

        // DELETE: api/Lesson/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id) {
            var query = $"DELETE FROM Lesson WHERE lesson_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                // No rows affected check can be implemented here.
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the lesson.", Details = ex.Message });
            }
        }
    }
}
