using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UnitApiController : ControllerBase {
        private readonly DataService _dataService;

        public UnitApiController(DataService dataService) {
            _dataService = dataService;
        }

        // GET: api/Unit
        [HttpGet]
        public async Task<IActionResult> GetUnits() {
            var query = "SELECT unit_id, module_id, title, description, sequence FROM Unit";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                var units = new List<object>();

                while (await reader.ReadAsync()) {
                    units.Add(new {
                        UnitId = reader.GetInt32(0),
                        ModuleId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Sequence = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4)
                    });
                }

                return Ok(units);
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching units.", Details = ex.Message });
            }
        }

        // GET: api/Unit/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUnitById(int id) {
            var query = $"SELECT unit_id, module_id, title, description, sequence FROM Unit WHERE unit_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var unit = new {
                        UnitId = reader.GetInt32(0),
                        ModuleId = reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Sequence = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4)
                    };

                    return Ok(unit);
                } else {
                    return NotFound(new { Message = "Unit not found." });
                }
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching the unit.", Details = ex.Message });
            }
        }

        // POST: api/Unit
        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromBody] JsonElement body) {
            if (!body.TryGetProperty("module_id", out JsonElement moduleIdElement) || !body.TryGetProperty("title", out JsonElement titleElement)) {
                return BadRequest(new { Message = "Invalid input. 'module_id' and 'title' are required." });
            }

            var moduleId = moduleIdElement.GetInt32();
            var title = titleElement.GetString();
            var description = body.TryGetProperty("description", out JsonElement descriptionElement) ? descriptionElement.GetString() : null;
            var sequence = body.TryGetProperty("sequence", out JsonElement sequenceElement) ? sequenceElement.GetInt32() : (int?)null;

            var query = "INSERT INTO Unit (module_id, title, description, sequence) " +
                        $"VALUES ({moduleId}, '{title}', '{description}', {sequence}) RETURNING unit_id";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var unitId = reader.GetInt32(0);
                    return CreatedAtAction(nameof(GetUnitById), new { id = unitId }, new { UnitId = unitId, ModuleId = moduleId, Title = title, Description = description, Sequence = sequence });
                }

                return StatusCode(500, new { Message = "An error occurred while creating the unit." });
            } catch (PostgresException ex) when (ex.SqlState == "23503") // Foreign key violation
              {
                return BadRequest(new { Message = "The specified module_id does not exist." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the unit.", Details = ex.Message });
            }
        }

        // DELETE: api/Unit/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit(int id) {
            var query = $"DELETE FROM Unit WHERE unit_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                // No rows affected check can be implemented here.
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the unit.", Details = ex.Message });
            }
        }
    }
}

