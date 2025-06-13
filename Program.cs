using healthmate_backend.Models;
using healthmate_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dotenv.net;
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        policy.WithOrigins("http://localhost:64138", "http://localhost:5181") // Added localhost:5181
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// Your MySQL connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "server=mysql-2dbc541a-healthmate.k.aivencloud.com;port=15855;database=defaultdb;user=avnadmin;password=AVNS_lhFXyGW3wGvurVtSCVi;SslMode=Required;";

// Add DbContext with retry policy
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 35)),
        mySqlOptions => mySqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null)
    );
});
builder.Services.AddScoped<ImageService>();

// Add AuthenticationService
builder.Services.AddScoped<AuthenticationService>();

// add Reminder Service 
builder.Services.AddScoped<ReminderService>();
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddScoped<GeoLocationService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpService>();

var app = builder.Build();

// Comment out or remove this line during development
// app.UseHttpsRedirection();

app.UseCors("AllowFlutterWeb");
// Middleware
app.UseAuthentication();  // 🔥 Add before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
