using System.Text;
using GenericAuth.API.Middleware;
using GenericAuth.Application;
using GenericAuth.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Application layer services (MediatR, FluentValidation, AutoMapper)
builder.Services.AddApplication();

// Add Infrastructure layer services (DbContext, Repositories, Identity)
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Auth Admin only policy
    options.AddPolicy("AuthAdminOnly", policy =>
        policy.RequireClaim("user_type", "AuthAdmin"));

    // Require application context (for regular users)
    options.AddPolicy("RequireApplication", policy =>
        policy.RequireClaim("application_id"));

    // Require specific application role
    options.AddPolicy("RequireApplicationAdmin", policy =>
        policy.RequireClaim("application_role", "Admin"));
});

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configure Swagger for API versioning
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GenericAuth API",
        Version = "v1",
        Description = "Multi-Tenant Authentication & Authorization API - Version 1",
        Contact = new OpenApiContact
        {
            Name = "GenericAuth Team"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add Application authentication headers to Swagger
    c.AddSecurityDefinition("ApplicationAuth", new OpenApiSecurityScheme
    {
        Description = "Application authentication using X-Application-Code and X-Api-Key headers",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Application-Code"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS (configure based on your requirements)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed the database (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var seeder = services.GetRequiredService<GenericAuth.Infrastructure.Persistence.DatabaseSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GenericAuth API V1");
        c.RoutePrefix = string.Empty; // Swagger at root
        c.DisplayRequestDuration(); // Show request duration in Swagger UI
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Add Application Authentication Middleware (validates X-Application-Code and X-Api-Key)
// This must come BEFORE UseAuthentication
app.UseApplicationAuthentication();

// Add Authentication & Authorization
app.UseAuthentication();

// Global exception handler to catch validation exceptions
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        if (exception is FluentValidation.ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = validationException.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors,
                Data = (object?)null
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // For other exceptions, return 500
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Success = false,
            Message = "An error occurred while processing your request.",
            Errors = new[] { exception?.Message ?? "Unknown error" },
            Data = (object?)null
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();

// Make the implicit Program class public for integration testing
public partial class Program { }
