using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QRAttendance.API.Data;
using QRAttendance.API.Repositories;
using QRAttendance.API.Services;
using QRAttendanceAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// KESTREL (ENABLE HTTP FOR MOBILE)
// ==============================
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5041); // HTTP (mobile-friendly)
    options.ListenAnyIP(7041, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS (browser/dev)
    });
});

// ==============================
// CONTROLLERS & SWAGGER
// ==============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "QR API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ==============================
// DAPPER / SERVICES
// ==============================
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<DashboardRepository>();


// ==============================
// JWT AUTH CONFIG
// ==============================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key is missing in configuration.");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // IMPORTANT for HTTP
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "https://localhost:5001", // MudBlazor dev
                    "http://localhost:5001"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();

// ==============================
// MIDDLEWARE
// ==============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ DO NOT redirect HTTPS (breaks mobile)
// app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();