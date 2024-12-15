using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;
using DirectDbWebApp.Tools;


[ApiController]
[Route("auth")]
public class AuthController : Controller {


    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(string username, string password) {
        // Validate the user's credentials using your database
        if (!AuthenticateUser(username, password)) {
            return Unauthorized("Invalid username or password");
        }

        // Create claims for the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            // Add more claims as needed, e.g., roles
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



    public void RegisterUser(string username, string password) {
        string salt = PasswordHasher.GenerateSalt();
        string hashedPassword = PasswordHasher.HashPassword(password, salt);

        using (var connection = new NpgsqlConnection("YourConnectionString")) {
            connection.Open();
            string query = "INSERT INTO users (username, password_hash, salt) VALUES (@username, @passwordHash, @salt)";
            using (var cmd = new NpgsqlCommand(query, connection)) {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("passwordHash", hashedPassword);
                cmd.Parameters.AddWithValue("salt", salt);
                cmd.ExecuteNonQuery();
            }
        }
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok("Logged out successfully");
    }

    private bool AuthenticateUser(string username, string password) {
        using (var connection = new NpgsqlConnection("YourConnectionString")) {
            connection.Open();
            string query = "SELECT password_hash, salt FROM users WHERE username = @username";
            using (var cmd = new NpgsqlCommand(query, connection)) {
                cmd.Parameters.AddWithValue("username", username);
                using (var reader = cmd.ExecuteReader()) {
                    if (reader.Read()) {
                        string storedHash = reader.GetString(0);
                        string salt = reader.GetString(1);
                        string enteredHash = PasswordHasher.HashPassword(password, salt);

                        return storedHash == enteredHash; // Match password
                    }
                }
            }
        }
        return false; // User not found
    }
}
