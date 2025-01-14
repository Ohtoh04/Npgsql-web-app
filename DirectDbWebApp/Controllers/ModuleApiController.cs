using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;
using System.Reflection;
using DirectDbWebApp.Domain;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ModuleApiController : ControllerBase {
        private readonly DataService _dataService;
        private readonly string _connectionString;

        public ModuleApiController(DataService dataService, IConfiguration configuration) {
            _dataService = dataService;
            this._connectionString = configuration.GetValue<string>("ConnectionString");
        }

        // GET: api/Module
        [HttpGet]
        public async Task<IActionResult> GetModules() {
            var query = "SELECT module_id, course_id, title, description, sequence FROM Module";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                var modules = new List<object>();

                while (await reader.ReadAsync()) {
                    modules.Add(new {
                        ModuleId = reader.GetInt32(0),
                        CourseId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Sequence = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4)
                    });
                }

                return Ok(modules);
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching modules.", Details = ex.Message });
            }
        }

        // GET: api/Module/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetModuleById(int id) {
            var query = $"SELECT module_id, course_id, title, description, sequence FROM Module WHERE module_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var module = new {
                        ModuleId = reader.GetInt32(0),
                        CourseId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Sequence = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4)
                    };

                    return Ok(module);
                } else {
                    return NotFound(new { Message = "Module not found." });
                }
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching the module.", Details = ex.Message });
            }
        }

        // POST: api/Module
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] CourseModule module) {

            var courseId = module.CourseId;
            var title = module.Title;
            var description = module.Description;
            var sequence = module.Sequence;

            var query = "INSERT INTO Module (course_id, title, description, sequence) " +
                        $"VALUES ({courseId}, '{title}', '{description}', {sequence}) RETURNING module_id";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var moduleId = reader.GetInt32(0);
                    return CreatedAtAction(nameof(GetModuleById), new { id = moduleId }, new { ModuleId = moduleId, CourseId = courseId, Title = title, Description = description, Sequence = sequence });
                }

                return StatusCode(500, new { Message = "An error occurred while creating the module." });
            } catch (PostgresException ex) when (ex.SqlState == "23503") // Foreign key violation
              {
                return BadRequest(new { Message = "The specified course_id does not exist." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the module.", Details = ex.Message });
            }
        }

        // DELETE: api/Module/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModule(int id) {
            var query = $"DELETE FROM Module WHERE module_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                // No rows affected check can be implemented here.
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the module.", Details = ex.Message });
            }
        }
    }
}
