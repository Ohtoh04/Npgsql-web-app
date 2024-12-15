using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryApiController : ControllerBase {
        private readonly DataService _dataService;

        public CategoryApiController(DataService dataService) {
            _dataService = dataService;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetCategories() {
            var query = "SELECT category_id, name FROM Category";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                var categories = new List<object>();

                while (await reader.ReadAsync()) {
                    categories.Add(new {
                        CategoryId = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }

                return Ok(categories);
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching categories.", Details = ex.Message });
            }
        }

        // GET: api/Category/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id) {
            var query = $"SELECT category_id, name FROM Category WHERE category_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var category = new {
                        CategoryId = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };

                    return Ok(category);
                } else {
                    return NotFound(new { Message = "Category not found." });
                }
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching the category.", Details = ex.Message });
            }
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] JsonElement body) {
            if (!body.TryGetProperty("name", out JsonElement nameElement) || string.IsNullOrWhiteSpace(nameElement.GetString())) {
                return BadRequest(new { Message = "Invalid input. 'name' is required." });
            }

            var name = nameElement.GetString();
            var query = $"INSERT INTO Category (name) VALUES ('{name}') RETURNING category_id";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var categoryId = reader.GetInt32(0);
                    return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, new { CategoryId = categoryId, Name = name });
                }

                return StatusCode(500, new { Message = "An error occurred while creating the category." });
            } catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
              {
                return Conflict(new { Message = "A category with this name already exists." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the category.", Details = ex.Message });
            }
        }

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id) {
            var query = $"DELETE FROM Category WHERE category_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the category.", Details = ex.Message });
            }
        }
    }
}
