using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Services;
using HotelBookingSystem.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7111", "http://localhost:5125", "https://localhost:5125")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Decide storage type from configuration
var storageType = builder.Configuration["Storage:Type"] ?? "Database";

if (storageType.Equals("XML", StringComparison.OrdinalIgnoreCase))
{
    var xmlFilePath = builder.Configuration["Storage:XmlFilePath"] ?? "rooms.xml";
    builder.Services.AddScoped<IRoomRepository>(provider => new XmlRoomRepository(xmlFilePath));
    
    XmlRoomSeeder.SeedDefaultRoomsIfNeeded(xmlFilePath);
}
else
{
    builder.Services.AddDbContext<RoomDbContext>(options =>
        options.UseSqlite("Data Source=room.db"));
    builder.Services.AddScoped<IRoomRepository, DatabaseRoomRepository>();
}

// Register services
builder.Services.AddScoped<RoomManager>();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "HotelBookingSystem",
            ValidAudience = "HotelBookingSystemUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] 
                    ?? builder.Configuration["Jwt:Secret"] 
                    ?? "5V8wVD4WNUsVQqNyaCl04SXyb5Q56mAC"))
        };
    });

// Add MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Room Service API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});


var app = builder.Build();

// Debug: Always run database initialization
Console.WriteLine("=== DATABASE INITIALIZATION START ===");
Console.WriteLine($"Storage type from config: {storageType}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
    
    Console.WriteLine("DbContext created successfully");
    
    // Force database creation with current schema
    Console.WriteLine("Creating database with current schema...");
    var created = context.Database.EnsureCreated();
    Console.WriteLine($"Database created: {created}");
    
    // Check if rooms table is empty and seed default rooms
    try
    {
        var roomCount = await context.Rooms.CountAsync();
        Console.WriteLine($"Current room count: {roomCount}");
        
        if (roomCount == 0)
        {
            Console.WriteLine("Seeding default rooms...");
            await RoomSeedHelper.SeedDefaultRoomsAsync(context);
            Console.WriteLine("Default rooms seeded successfully!");
        }
        else
        {
            Console.WriteLine($"Database already has {roomCount} rooms, skipping seeding.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during seeding: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

Console.WriteLine("=== DATABASE INITIALIZATION END ===");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();
