using System.Data;
using MySqlConnector;
using Scalar.AspNetCore;
using Biblioteca;
using Biblioteca.Persistencia.Dapper;

var builder = WebApplication.CreateBuilder(args);

//  Obtener la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySQL");

//  Registrando IDbConnection para que se inyecte como dependencia
//  Cada vez que se inyecte, se creará una nueva instancia con la cadena de conexión
builder.Services.AddScoped<IDbConnection>(sp => new MySqlConnection(connectionString));

//Cada vez que necesite la interfaz, se va a instanciar automaticamente AdoDapper y se va a pasar al metodo de la API
builder.Services.AddScoped<IAdoAsync, AdoDapperAsync>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference();
}
app.MapGet("/electrodomestico/{id}", async (int id,IAdoAsync repo) =>
    await repo.ObtenerElectrodomestico(id)
        is Electrodomestico electrodomestico
            ? Results.Ok(electrodomestico)
            : Results.NotFound());

app.MapGet("")


app.Run();