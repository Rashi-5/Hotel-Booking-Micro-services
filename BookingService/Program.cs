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
    // Azure SQL or SQLite connection
    var connectionString = builder.Configuration.GetConnectionString("BookingDatabase") ?? "Data Source=booking.db";
    var isAzureSQL = connectionString.Contains("database.windows.net");
    
    if (isAzureSQL)
    {
        builder.Services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    else
    {
        builder.Services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlite(connectionString));
    }
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
        
        try
        {
            var connectionString = builder.Configuration.GetConnectionString("BookingDatabase") ?? "Data Source=booking.db";
            
            // Use Migrate for Azure SQL, EnsureCreated for SQLite
            if (connectionString.Contains("database.windows.net"))
            {
                Console.WriteLine("Applying database migrations for Azure SQL...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Azure SQL migrations completed");
            }
            else
            {
                var created = context.Database.EnsureCreated();
                Console.WriteLine($"SQLite database created: {created}");
            }
            
            // Check current booking count
            var bookingCount = await context.Bookings.CountAsync();
            Console.WriteLine($"Current booking count: {bookingCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization error: {ex.Message}");
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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();
