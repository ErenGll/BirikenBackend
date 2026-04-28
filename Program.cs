using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// --- RENDER BAĞLANTI AYARI (GÜNCELLENDİ) ---
var rawConn = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString = "";

if (string.IsNullOrEmpty(rawConn))
{
    // Yerel çalışma için appsettings'e bakar
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}
else if (rawConn.StartsWith("postgres://"))
{
    // Render formatını (postgres://user:pass@host:port/db) .NET formatına çevirir
    var databaseUri = new Uri(rawConn);
    var userInfo = databaseUri.UserInfo.Split(':');

    connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={databaseUri.LocalPath.TrimStart('/')};SslMode=Require;Trust Server Certificate=true";
}
else
{
    // Eğer link zaten doğru formattaysa direkt kullan
    connectionString = rawConn;
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
// ... geri kalan kodlar aynı ...