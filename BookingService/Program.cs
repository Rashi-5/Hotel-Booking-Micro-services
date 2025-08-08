using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Services;
using HotelBookingSystem.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelBookingSystem.Helper;
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

// Configure storage type
var storageType = builder.Configuration["Storage:Type"] ?? "Database";

if (storageType.Equals("XML", StringComparison.OrdinalIgnoreCase))
{
    var xmlFilePath = builder.Configuration["Storage:XmlFilePath"] ?? "bookings.xml";
    builder.Services.AddScoped<IBookingRepository>(provider => new XmlBookingRepository(xmlFilePath));
}
else
{
    // Database storage (default)
    builder.Services.AddDbContext<BookingDbContext>(options =>
        options.UseSqlite("Data Source=booking.db"));
    builder.Services.AddScoped<IBookingRepository, DatabaseBookingRepository>();
}

// Register services
builder.Services.AddScoped<BookingManager>();

// Register HttpClient for Room Service communication
builder.Services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>{
    client.BaseAddress = new Uri(builder.Configuration["RoomService:BaseUrl"] ?? "http://localhost:5238");
});

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

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking Service API", Version = "v1" });
    
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

// Ensure database is created and migrated
if (storageType.Equals("Database", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        
        // Ensure database is created with latest schema
        context.Database.EnsureDeleted(); // Remove old database
        context.Database.EnsureCreated(); // Create new database with current schema
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
