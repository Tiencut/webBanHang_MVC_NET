using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020761.BusinessLayers;

var builder = WebApplication.CreateBuilder(args);

// Initialize business layer configuration (connection string)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Configuration.Initialize(connectionString);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

// Ensure text responses include charset=utf-8 to avoid browser encoding issues
app.Use(async (context, next) =>
{
    await next();
    var ct = context.Response.ContentType;
    if (!string.IsNullOrEmpty(ct) && ct.StartsWith("text/") && !ct.Contains("charset", System.StringComparison.OrdinalIgnoreCase))
    {
        context.Response.ContentType = ct + "; charset=utf-8";
    }
});

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
