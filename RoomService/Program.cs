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

var configuration = builder.Configuration;
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
    // Azure SQL or SQLite connection
    var connectionString = builder.Configuration.GetConnectionString("RoomDatabase") ?? "Data Source=rooms.db";
    var isAzureSQL = connectionString.Contains("database.windows.net");
    
    if (isAzureSQL)
    {
        builder.Services.AddDbContext<RoomDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    else
    {
        builder.Services.AddDbContext<RoomDbContext>(options =>
            options.UseSqlite(connectionString));
    }
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

// Database initialization only for Database storage type
if (storageType.Equals("Database", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Storage type from config: {storageType}");

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
                
        var connectionString = builder.Configuration.GetConnectionString("RoomDatabase") ?? "Data Source=rooms.db";
        
        // Use Migrate for Azure SQL, EnsureCreated for SQLite
        if (connectionString.Contains("database.windows.net"))
        {
            Console.WriteLine("Applying database migrations for Azure SQL...");
            await context.Database.MigrateAsync();
            Console.WriteLine("Azure SQL migrations completed");
        }
        else
        {
            // Force database creation with current schema for SQLite
            var created = context.Database.EnsureCreated();
            Console.WriteLine($"SQLite database created: {created}");
        }
        
        // Check if rooms table is empty and seed default rooms
        try
        {
            var roomCount = await context.Rooms.CountAsync();
            Console.WriteLine($"Current room count: {roomCount}");
            
            if (roomCount == 0)
            {
                Console.WriteLine("Seeding default rooms from XML...");
                var defaultRoomsXmlPath = configuration.GetValue<string>("Storage:DefaultRoomsXmlPath") ?? "Data/default-rooms.xml";
                Console.WriteLine($"Using XML file: {defaultRoomsXmlPath}");
                await RoomSeedHelper.SeedDefaultRoomsAsync(context, defaultRoomsXmlPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during seeding: {ex.Message}");
        }
    }
}

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
