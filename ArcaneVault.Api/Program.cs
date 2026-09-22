// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Services
// ---------------------------------------------------------------
builder.Services.AddControllers();

// Entity Framework Core connected to the SQLite database file (arcanevault.db).
// The connection string lives in appsettings.json under ConnectionStrings:DefaultConnection.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core's built-in PBKDF2 password hasher. Registered as a singleton because it
// is stateless. Used by AccountsController to hash on registration and verify on login,
// so the database never contains a plain-text password.
builder.Services.AddSingleton<IPasswordHasher<ArcaneVaultUser>, PasswordHasher<ArcaneVaultUser>>();

// Swagger UI - lets the Web API be demonstrated and tested independently of the Razor front end.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Arcane Vault API",
        Version = "v1",
        Description = "Data and analytics API backing the Arcane Vault collection-management prototype."
    });
});

var app = builder.Build();

// ---------------------------------------------------------------
// Startup: apply migrations, then seed demo data if the database is empty.
// ---------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ArcaneVaultUser>>();
    db.Database.Migrate();
    DbInitializer.Seed(db, hasher);
}

// ---------------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Arcane Vault API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
