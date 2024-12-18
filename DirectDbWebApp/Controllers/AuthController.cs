using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;
using DirectDbWebApp.Tools;
using DirectDbWebApp.Services;


[ApiController]
[Route("auth")]
public class AuthController : Controller {
    private readonly DataService _dataService;
    private readonly string _connectionString;
    public AuthController(DataService DataService, IConfiguration Config) { 
        this._dataService = DataService;
        this._connectionString = Config.GetValue<string>("ConnectionString") ?? "";
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromForm] string username, [FromForm] string password) {
        string role = "Student";
        // Validate the user's credentials using your database
        var UserId = await AuthenticateUser(username, password);
        if (UserId==-1) {
            return Unauthorized("Invalid username or password");
        }

        // Create claims for the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Create the authentication properties
        var authProperties = new AuthenticationProperties {
            IsPersistent = true, // Keep cookie active even after closing the browser
        };

        // Sign in and issue the cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok("Login successful");
    }

    [HttpGet("login")]
    public IActionResult Login() {
        return View();
    }

    [HttpGet("register")]
    public IActionResult Register() {
        Console.WriteLine("register view function");

        return View();
    }


    [HttpPost("registeruser")]
    public async Task<IActionResult> RegisterUser([FromForm] string username, [FromForm] string email, [FromForm] string password) {
        string hashedPassword = PasswordHasher.HashPassword(password);
        string query = $"INSERT INTO dbuser (username, email, password_hash) " +
                       $"VALUES (@username, @email, @password) RETURNING user_id;";

        try {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Set up the command with parameters to prevent SQL Injection
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("password", hashedPassword);

            // Execute the query and get the result (RETURNING user_id)
            var userId = await cmd.ExecuteScalarAsync();

            if (userId != null) {
                return Redirect("/auth/login");
            }

            return BadRequest(new { message = "Failed to create user" });
        } catch (Exception ex) {
            return StatusCode(500, new { message = ex.Message });
        }
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok("Logged out successfully");
    }


    // auxilliary function that returns -1 if user doesnt exist or credentials dont match and UserId if authentication is successful
    private async Task<int> AuthenticateUser(string username, string password) {
        var query = "SELECT user_id,password_hash FROM dbuser WHERE username = @username";

        try {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                var storedHash = reader["password_hash"].ToString();
                var UserId = (int)reader["user_id"];
                string enteredHash = PasswordHasher.HashPassword(password);
                return storedHash == enteredHash ? UserId : -1;
            }

            return -1;
        } catch (Exception ex) {
            Console.WriteLine(ex);
            return -1;
        }
    }

}
