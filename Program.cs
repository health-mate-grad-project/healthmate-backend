using healthmate_backend.Models;
using healthmate_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Your MySQL connection string
var connectionString = "server=mysql-2dbc541a-healthmate.k.aivencloud.com;port=15855;database=defaultdb;user=avnadmin;password=AVNS_lhFXyGW3wGvurVtSCVi;SslMode=Required;";

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 35))
    )
);

// Add AuthenticationService
builder.Services.AddScoped<AuthenticationService>();

// Add IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key", "JWT Key is not configured");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
    	ValidateLifetime = true,
    	RoleClaimType = "Role"
    };
});

// Add Controllers
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<DoctorHomeScreenService>();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseAuthentication();  // 🔥 Add before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
