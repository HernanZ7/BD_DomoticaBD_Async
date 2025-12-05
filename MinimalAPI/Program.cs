using System.Data;
using MySqlConnector;
using Scalar.AspNetCore;
using Biblioteca;
using Biblioteca.Persistencia.Dapper;
using MinimalApi.Dtos;

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

builder.Services.AddControllers();
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
app.UseHttpsRedirection();
app.UseAuthorization();

// Mapear controllers
app.MapControllers();
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


app.MapPost("/casa/dto", async (CrearCasaRequest request, IAdoAsync repo) =>
{
    var nuevaCasa = new Casa { Direccion = request.Direccion };
    await repo.AltaCasaAsync(nuevaCasa);

    var response = new CasaResponse(nuevaCasa.IdCasa, nuevaCasa.Direccion);
    return Results.Created($"/casa/{nuevaCasa.IdCasa}", response);
});

app.MapPost("/electrodomestico/dto", async (CrearElectrodomesticoRequest request, IAdoAsync repo) =>
{
    var nuevoElectro = new Electrodomestico
    {
        IdCasa = request.IdCasa,
        Nombre = request.Nombre,
        Tipo = request.Tipo,
        Ubicacion = request.Ubicacion,
        Encendido = request.Encendido,
    };

    await repo.AltaElectrodomesticoAsync(nuevoElectro);

    var response = new ElectrodomesticoResponse(
        nuevoElectro.IdElectrodomestico,
        nuevoElectro.IdCasa,
        nuevoElectro.Nombre,
        nuevoElectro.Tipo,
        nuevoElectro.Ubicacion,
        nuevoElectro.Encendido
    );

    return Results.Created($"/electrodomestico/{nuevoElectro.IdElectrodomestico}", response);
});

app.MapGet("/casa/dto/{id}", async (int id, IAdoAsync repo) =>
{
    var casa = await repo.ObtenerCasaAsync(id);

    if (casa is null)
        return Results.NotFound();

    var response = new CasaResponse(casa.IdCasa, casa.Direccion);
    return Results.Ok(response);
});

app.MapGet("/electrodomestico/dto/{id}", async (int id, IAdoAsync repo) =>
{
    var electro = await repo.ObtenerElectrodomesticoAsync(id);

    if (electro is null)
        return Results.NotFound();

    var response = new ElectrodomesticoResponse(
        electro.IdElectrodomestico,
        electro.IdCasa,
        electro.Nombre,
        electro.Tipo,
        electro.Ubicacion,
        electro.Encendido
    );

    return Results.Ok(response);
});

app.Run();