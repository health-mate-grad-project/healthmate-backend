using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // <-- Important

var builder = WebApplication.CreateBuilder(args);

// Your MySQL connection string
var connectionString = "server=mysql-2dbc541a-healthmate.k.aivencloud.com;port=15855;database=defaultdb;user=avnadmin;password=AVNS_lhFXyGW3wGvurVtSCVi;SslMode=Required;";

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 35)) // <-- your MySQL server version
    )
);

var app = builder.Build();

app.Run();
