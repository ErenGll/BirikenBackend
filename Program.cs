using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// --- RENDER BAĞLANTI AYARI ---
var rawConn = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString = "";

if (string.IsNullOrEmpty(rawConn))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}
else if (rawConn.StartsWith("postgres://"))
{
    // Render formatını .NET formatına dönüştüren en güvenli yöntem
    var databaseUri = new Uri(rawConn);
    var userInfo = databaseUri.UserInfo.Split(':');
    connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={databaseUri.LocalPath.TrimStart('/')};SslMode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = rawConn;
}

// Servisleri Kaydet
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Veritabanını Otomatik Oluştur
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Middleware Yapılandırması
app.UseSwagger();
app.UseSwaggerUI(c => { 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Biriken API v1"); 
    c.RoutePrefix = string.Empty; 
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

// --- MODELLER VE DB CONTEXT (Hata almamak için aynı dosyada kalsınlar) ---
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