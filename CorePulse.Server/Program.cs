using CorePulse.Server.Hubs;
using CorePulse.Server.Infrastructure;
using CorePulse.Server.Infrastructure.Middlewares;
using CorePulse.Server.Infrastructure.Repositories.Implementations;
using CorePulse.Server.Infrastructure.Repositories.Interfaces;
using CorePulse.Server.Services.Implementations;
using CorePulse.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CORE SERVICES ---
// Adds support for Web API controllers
builder.Services.AddControllers();

// Configures OpenAPI/Swagger for API documentation and testing
builder.Services.AddOpenApi();

// --- 2. DATABASE CONFIGURATION ---
// Registers the Entity Framework Core DbContext using SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

// --- 3. DEPENDENCY INJECTION (Repositories & Services) ---
// Repository for handling user-related database operations
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Service for handling user authentication logic (Login/Register)
builder.Services.AddScoped<IAuthService, AuthService>();

// Service for generating and validating JWT tokens
builder.Services.AddScoped<ITokenService, TokenService>();

// --- 4. AUTHENTICATION & JWT CONFIGURATION ---
// Configures JWT Bearer authentication to protect API endpoints and SignalR Hubs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Ensures the token was signed by our server
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            ValidateIssuer = true, // Validates the server that issued the token
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true, // Validates that the token is intended for this app
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true, // Checks if the token has expired
            ClockSkew = TimeSpan.FromMinutes(5) // Allows 5-minute margin for server time differences
        };

        // Specific configuration for SignalR to read tokens from the QueryString
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/metricsHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// --- 5. MONITORING & REAL-TIME SERVICES ---
// Singleton cache to store process data and share it across the application
builder.Services.AddSingleton<ProcessCacheService>();

// Background worker that continuously monitors system metrics (CPU, RAM)
builder.Services.AddHostedService<SystemMonitorService>();

// Adds SignalR for real-time bi-directional communication
builder.Services.AddSignalR()
    .AddJsonProtocol(options => {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });

// --- 6. CORS POLICY ---
// Configures Cross-Origin Resource Sharing for the Blazor Client
builder.Services.AddCors(options => {
    options.AddPolicy("BlazorCorsPolicy", policy => {
        policy.WithOrigins("https://localhost:7162") // Allows only our Blazor app
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR over HTTPS
    });
});

var app = builder.Build();

// --- 7. HTTP REQUEST PIPELINE ---

if (app.Environment.IsDevelopment())
{
    // Enables OpenAPI visualization in development mode
    app.MapOpenApi();
}

// Redirects all HTTP requests to HTTPS
app.UseHttpsRedirection();

// Custom Middleware for global exception handling
app.UseMiddleware<ErrorHandlingMiddleware>();

// Applies the CORS policy defined earlier
app.UseCors("BlazorCorsPolicy");

// Enables authentication (Who is the user?)
app.UseAuthentication();

// Enables authorization (What is the user allowed to do?)
app.UseAuthorization();

// --- 8. ENDPOINT MAPPING ---
// Maps the SignalR Hub endpoint
app.MapHub<MetricsHub>("/metricsHub");

// Maps Controller routes (e.g., api/[controller])
app.MapControllers();

// Starts the application
app.Run();