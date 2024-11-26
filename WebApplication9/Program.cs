using Microsoft.EntityFrameworkCore;
using WebApplication9.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SistemaDeConcursosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Conn")));

// Configuraci�n de la sesi�n y cookies
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Asegura que la cookie de sesi�n se env�a incluso si el usuario ha optado por no ser rastreado
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// Habilita el uso de sesiones antes de la autorizaci�n
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");


app.Run();
