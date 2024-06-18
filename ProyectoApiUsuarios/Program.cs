using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ProyectoApiUsuarios.Services;
using System;
using System.Text;
using MongoDBSettings = ProyectoApiUsuarios.Services.MongoDatabaseSettings;

var builder = WebApplication.CreateBuilder(args);

// Configuración de MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection"));
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(60); // Aumenta el tiempo de espera a 60 segundos
    settings.ConnectTimeout = TimeSpan.FromSeconds(30); // Configura el tiempo de espera de conexión
    settings.SocketTimeout = TimeSpan.FromSeconds(30); // Configura el tiempo de espera del socket
    settings.UseTls = true; // Asegura que se usa SSL/TLS
    return new MongoClient(settings);
});

builder.Services.AddSingleton<IMongoDatabaseSettings>(sp =>
    new MongoDBSettings
    {
        ConnectionString = builder.Configuration.GetConnectionString("MongoDbConnection"),
        DatabaseName = builder.Configuration.GetValue<string>("MongoDatabaseName")
    });

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<MedicoService>();
builder.Services.AddSingleton<PatientService>();
builder.Services.AddHttpClient();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(options =>
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
