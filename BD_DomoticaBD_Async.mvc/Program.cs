// BD_DomoticaBD_Async.mvc/Program.cs

using System.Data;
using Biblioteca;
using Biblioteca.Persistencia.Dapper;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//  Obtener la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySQL");

builder.Services.AddScoped<IDbConnection>(sp => new MySqlConnection(connectionString));
builder.Services.AddScoped<IAdoAsync, AdoDapperAsync>();

// ---- AÑADIR: sesión ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".IntelHome.Session";
    options.IdleTimeout = TimeSpan.FromHours(8); // ajustar si querés
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ---- FIN: sesión ----

var app = builder.Build();

// Middleware básico
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ---- AÑADIR: usar session ----
app.UseSession();
// ---- FIN: usar session ----

app.UseAuthorization();

// 👇 Aquí está la clave: configurar la ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
