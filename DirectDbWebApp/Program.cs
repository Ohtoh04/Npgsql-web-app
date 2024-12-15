using DirectDbWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetValue<string>("ConnectionString") ?? "";

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DataService>(provider => new DataService(connectionString));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/auth/login"; // Redirect here if not authenticated
        options.LogoutPath = "/auth/logout"; // Redirect here on logout
        options.ExpireTimeSpan = TimeSpan.FromHours(1); // Cookie expiration
        options.SlidingExpiration = true; // Renew cookie if active
    });

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();   // добавление middleware аутентификации 
app.UseAuthorization();   // добавление middleware авторизации 

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
