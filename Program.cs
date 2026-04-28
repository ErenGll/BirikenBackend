using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// --- RENDER BAĞLANTI AYARI ---
var rawConn = Environment.GetEnvironmentVariable("DATABASE_URL") 
              ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString = rawConn ?? "";

if (!string.IsNullOrEmpty(rawConn) && rawConn.StartsWith("postgres://"))
{
    var uri = new Uri(rawConn);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.LocalPath.TrimStart('/')};SslMode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Biriken API v1"); c.RoutePrefix = string.Empty; });
app.UseCors("AllowAll");
app.MapControllers();
app.Run();

// --- MODELLER (BURADA KALSINLAR Kİ HATA ÇIKMASIN) ---
public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
}