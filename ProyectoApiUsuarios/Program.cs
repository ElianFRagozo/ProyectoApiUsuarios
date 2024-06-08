using MongoDB.Driver;
using ProyectoApiUsuarios.Controllers;
using ProyectoApiUsuarios.services;
using ProyectoApiUsuarios.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoDatabaseSettings>(sp =>
    new ProyectoApiUsuarios.Controllers.MongoDatabaseSettings
    {
        ConnectionString = "mongodb+srv://eenriquefragozo:PRaCUGb0aXeffjYF@cluster0.uawckuf.mongodb.net/?retryWrites=true&w=majority&appName=Cluster",
        DatabaseName = "User"
    });

// Crea una instancia de PatientService y la registra como un servicio singleton
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<MedicoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
