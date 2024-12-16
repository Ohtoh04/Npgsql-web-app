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
    public AuthController(DataService DataService) { 
        this._dataService = DataService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(string username, string password, string role) {
        // Validate the user's credentials using your database
        if (!(await AuthenticateUser(username, password))) {
            return Unauthorized("Invalid username or password");
        }

        // Create claims for the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
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
        return View();
    }



    public async Task<IActionResult> RegisterUser(string username, string password) {
        string hashedPassword = PasswordHasher.HashPassword(password);
        string query = $"INSERT INTO users (username, password_hash) VALUES ({username}, {hashedPassword})";

        try {
            await using var reader = await _dataService.ExecuteQuery(query);
            if (!(await reader.ReadAsync())) {
                return Created($"/api/users/{reader["user_id"]}", new { UserId = reader["user_id"] });
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

    private async Task<bool> AuthenticateUser(string username, string password) {
        var query = $"SELECT password_hash FROM users WHERE username = {username}";

        try {
            await using var reader = await _dataService.ExecuteQuery(query);
            if (await reader.ReadAsync()) {
                var storedHash = reader["password_hash"];
                string enteredHash = PasswordHasher.HashPassword(password);
                return (string)storedHash == enteredHash; // Match password
            }
            return false;
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }

        return false; // User not found
    }

}
