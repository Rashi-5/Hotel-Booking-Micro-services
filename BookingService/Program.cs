using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Services;
using HotelBookingSystem.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelBookingSystem.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    
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

// Register HttpClient for microservice communication
builder.Services.AddHttpClient<IBookingServiceClient, BookingServiceClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["BookingService:BaseUrl"] ?? "http://localhost:5211");
});

// Register HttpClient for Room Service communication
builder.Services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>{
    client.BaseAddress = new Uri(builder.Configuration["RoomService:BaseUrl"] ?? "http://localhost:5238");
});

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
                    ?? builder.Configuration["Jwt:Secret"]))
        };
    });

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
