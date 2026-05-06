using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// --- CORS AYARI (Netlify ile konuşabilmek için) ---
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// --- BAĞLANTI AYARI ---
string connectionString = "";
try {
    var rawConn = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(rawConn) && rawConn.StartsWith("postgres://")) {
        var uri = new Uri(rawConn);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.LocalPath.TrimStart('/')};SslMode=Require;Trust Server Certificate=true";
    } else {
        // Eğer Neon DB kullanıyorsan direkt adresi buraya tırnak içine de yazabilirsin kanka
        connectionString = rawConn ?? "Host=ep-sweet-hill-amyrp03u-pooler.c-5.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_ApL4OMthEs9C;SslMode=Require;Trust Server Certificate=true;";
    }
} catch (Exception ex) {
    Console.WriteLine("Bağlantı hatası: " + ex.Message);
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Veritabanı otomatik oluşturma
try {
    using (var scope = app.Services.CreateScope()) {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
} catch (Exception ex) {
    Console.WriteLine("DB Hatası: " + ex.Message);
}

app.UseSwagger();
app.UseSwaggerUI(c => { 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Biriken API v1"); 
    c.RoutePrefix = string.Empty; 
});

// SIRALAMA ÖNEMLİ: Routing -> CORS -> Authorization
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.Run();

// --- MODELLER ---
public class User {
    [Key] public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
}

// --- KONTROLCÜ ---
[ApiController] [Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _context;
    public AuthController(AppDbContext context) { _context = context; }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user) {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username)) 
            return BadRequest("Bu kullanıcı adı alınmış.");
            
        _context.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Kayıt başarılı!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(User loginUser) {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginUser.Username && u.Password == loginUser.Password);
        if (user == null) return Unauthorized("Hatalı giriş!");
        return Ok(new { message = "Hoş geldin!", userId = user.Id });
    }
}
