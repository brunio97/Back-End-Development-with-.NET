using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserManagementApi.Data;
using UserManagementApi.Middleware;
using UserManagementApi.Models;
using UserManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);


// -----------------------------------------------------
// Controllers
// -----------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();


// -----------------------------------------------------
// Database - Entity Framework Core + SQLite
// -----------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration
                .GetConnectionString("DefaultConnection")));


// -----------------------------------------------------
// ASP.NET Core Identity
// -----------------------------------------------------

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            // User
            options.User.RequireUniqueEmail = true;

            // Password
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // Lockout protection
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(5);

            // Not required for this assignment
            options.SignIn.RequireConfirmedEmail = false;
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// -----------------------------------------------------
// JWT Authentication
// -----------------------------------------------------

string jwtKeyBase64 =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is missing.");

byte[] jwtKey =
    Convert.FromBase64String(jwtKeyBase64);


builder.Services
    .AddAuthentication(options =>
    {
        // Force ASP.NET Core to use JWT instead
        // of Identity cookies for API authentication.

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultForbidScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,


                ValidIssuer =
                    builder.Configuration[
                        "Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration[
                        "Jwt:Audience"],


                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        jwtKey),


                RoleClaimType =
                    ClaimTypes.Role,

                NameClaimType =
                    ClaimTypes.NameIdentifier,


                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });


// -----------------------------------------------------
// Authorization
// -----------------------------------------------------

builder.Services.AddAuthorization();


// -----------------------------------------------------
// Application Services
// -----------------------------------------------------

builder.Services.AddScoped<JwtService>();


// -----------------------------------------------------
// Swagger + JWT Bearer Authentication
// -----------------------------------------------------

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "User Management API",
            Version = "v1"
        });


    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In =
                ParameterLocation.Header,

            Description =
                "Enter your JWT access token."
        });


    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType
                                    .SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// -----------------------------------------------------
// Build Application
// -----------------------------------------------------

var app = builder.Build();


// -----------------------------------------------------
// Database migrations + Roles/Admin Seed
// -----------------------------------------------------

using (var scope =
       app.Services.CreateScope())
{
    var database =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    await database.Database.MigrateAsync();

    await SeedData.InitializeAsync(
        scope.ServiceProvider,
        app.Configuration);

    if (app.Environment.IsDevelopment())
    {
        await OrderSeedData.InitializeAsync(
            database);
    }
}


// -----------------------------------------------------
// HTTP Pipeline
// -----------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


// Custom logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();


// IMPORTANT:
// Authentication must be before Authorization
app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();


app.Run();


// -----------------------------------------------------
// Required for integration tests
// -----------------------------------------------------

public partial class Program
{
}
