using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;
using DirectDbWebApp.Domain;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api/category")]
    public class CategoryApiController : ControllerBase {
        private readonly string _connectionString;

        public CategoryApiController(string connectionString) {
            _connectionString = connectionString;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetCategories() {
            var query = "SELECT category_id, name FROM Category";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                var categories = new List<Category>();

                while (await reader.ReadAsync()) {
                    categories.Add(new Category {
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
            var query = "SELECT category_id, name FROM Category WHERE category_id = @id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync()) {
                    var category = new Category {
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
        public async Task<IActionResult> CreateCategory([FromBody] Category category) {
            if (string.IsNullOrWhiteSpace(category.Name)) {
                return BadRequest(new { Message = "Invalid input. 'name' is required." });
            }

            var query = "INSERT INTO Category (name) VALUES (@name) RETURNING category_id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", category.Name);

                var categoryId = (int)await cmd.ExecuteScalarAsync();

                return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, new { CategoryId = categoryId, Name = category.Name });
            } catch (PostgresException ex) when (ex.SqlState == "23505") { // Unique violation
                return Conflict(new { Message = "A category with this name already exists." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the category.", Details = ex.Message });
            }
        }

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id) {
            var query = "DELETE FROM Category WHERE category_id = @id";

            try {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0) {
                    return NoContent();
                } else {
                    return NotFound(new { Message = "Category not found." });
                }
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the category.", Details = ex.Message });
            }
        }
    }

}
