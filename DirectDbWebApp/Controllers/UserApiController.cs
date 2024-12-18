using Microsoft.AspNetCore.Mvc;
using DirectDbWebApp.Services;
using System.Data;
using System.Text.Json;
using Npgsql;
using DirectDbWebApp.Domain;

namespace DirectDbWebApp.Controllers {
    [ApiController]
    [Route("api")]
    public class UserApiController : ControllerBase {
        private readonly DataService _dataService;
        private readonly string _connectionString;

        public UserApiController(DataService dataService, IConfiguration Config) {
            _dataService = dataService;
            this._connectionString = Config.GetValue<string>("ConnectionString") ?? "";
        }

        // GET: api/User
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() {
            var query = "SELECT user_id, username, email, first_name, last_name, bio, role, profile_picture, date_registered FROM DbUser";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);
                var users = new List<object>();

                while (await reader.ReadAsync()) {
                    users.Add(new {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        FirstName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        LastName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Bio = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Role = reader.GetString(6),
                        ProfilePicture = reader.IsDBNull(7) ? null : reader.GetString(7),
                        DateRegistered = reader.GetDateTime(8)
                    });
                }

                return Ok(users);
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching users.", Details = ex.Message });
            }
        }

        // GET: api/user/{id}
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id) {
            var query = $"SELECT user_id, username, email, first_name, last_name, bio, role, profile_picture, date_registered FROM DbUser WHERE user_id = {id}";

            try {
                NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new NpgsqlCommand(query, conn);
                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync()) {
                    var user = new DbUser {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        FirstName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        LastName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Bio = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Role = reader.GetString(6),
                        ProfilePicture = reader.IsDBNull(7) ? null : reader.GetString(7),
                        DateRegistered = reader.GetDateTime(8)
                    };

                    return Ok(user);
                } else {
                    return NotFound(new { Message = "User not found." });
                }
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while fetching the user.", Details = ex.Message });
            }
        }

        // POST: api/User
        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody] JsonElement body) {
            if (!body.TryGetProperty("username", out JsonElement usernameElement) || !body.TryGetProperty("email", out JsonElement emailElement) || !body.TryGetProperty("password_hash", out JsonElement passwordHashElement)) {
                return BadRequest(new { Message = "Invalid input. 'username', 'email', and 'password_hash' are required." });
            }

            var username = usernameElement.GetString();
            var email = emailElement.GetString();
            var passwordHash = passwordHashElement.GetString();
            var firstName = body.TryGetProperty("first_name", out JsonElement firstNameElement) ? firstNameElement.GetString() : null;
            var lastName = body.TryGetProperty("last_name", out JsonElement lastNameElement) ? lastNameElement.GetString() : null;
            var bio = body.TryGetProperty("bio", out JsonElement bioElement) ? bioElement.GetString() : null;
            var role = body.TryGetProperty("role", out JsonElement roleElement) ? roleElement.GetString() : "Student";
            var profilePicture = body.TryGetProperty("profile_picture", out JsonElement profilePictureElement) ? profilePictureElement.GetString() : null;

            var query = "INSERT INTO DbUser (username, email, first_name, last_name, bio, role, password_hash, profile_picture) " +
                        $"VALUES ('{username}', '{email}', '{firstName}', '{lastName}', '{bio}', '{role}', '{passwordHash}', '{profilePicture}') RETURNING user_id";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                if (await reader.ReadAsync()) {
                    var userId = reader.GetInt32(0);
                    return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { UserId = userId, Username = username, Email = email, FirstName = firstName, LastName = lastName, Bio = bio, Role = role, ProfilePicture = profilePicture });
                }

                return StatusCode(500, new { Message = "An error occurred while creating the user." });
            } catch (PostgresException ex) when (ex.SqlState == "23505") // Unique constraint violation
              {
                return BadRequest(new { Message = "The username or email already exists." });
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while creating the user.", Details = ex.Message });
            }
        }

        // DELETE: api/user/{id}
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id) {
            var query = $"DELETE FROM DbUser WHERE user_id = {id}";

            try {
                await using var reader = await _dataService.ExecuteQuery(query);

                // No rows affected check can be implemented here.
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while deleting the user.", Details = ex.Message });
            }
        }
    }
}
