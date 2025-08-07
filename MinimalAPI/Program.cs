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
app.MapGet("/electrodomestico/{id}", async (int id, IAdoAsync repo) =>
{
    var electro = await repo.ObtenerElectrodomesticoAsync(id);
    return electro is not null ? Results.Ok(electro) : Results.NotFound();
});


app.MapGet("/electrodomestico", async (IAdoAsync repo) =>
{
    var lista = await repo.ObtenerTodosLosElectrodomesticosAsync();
    return Results.Ok(lista);
});

app.MapPost("/electrodomestico", async (Electrodomestico nuevo, IAdoAsync repo) =>
{
    await repo.AltaElectrodomesticoAsync(nuevo);
    return Results.Created($"/electrodomestico/{nuevo.IdElectrodomestico}", nuevo);
});



app.MapGet("/casa/{id}", async (int id, IAdoAsync repo) =>
{
    var casa = await repo.ObtenerCasaAsync(id);
    return casa is not null ? Results.Ok(casa) : Results.NotFound();
});


app.MapGet("/casa", async (IAdoAsync repo) =>
{
    var casas = await repo.ObtenerTodasLasCasasAsync();
    return Results.Ok(casas);
});

app.MapPost("/casa", async (Casa nuevo, IAdoAsync repo) =>
{
    await repo.AltaCasaAsync(nuevo);
    return Results.Created($"/casa/{nuevo.IdCasa}", nuevo);
});

app.Run();