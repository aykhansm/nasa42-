using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using CelestialOrrery.Services.Interfaces;  // Adjust according to your actual namespace
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using CelestialOrrery.Hubs;
using CelestialOrrery.Utils;
using CelestialOrrery.Models;
using CelestialOrrery.Services;
using System.Text.Json;
using System;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
var mongoDbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<MongoDBSettings>();
if (mongoDbSettings == null)
    throw new InvalidOperationException("Database settings are not configured properly.");

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IGameSessionService, GameSessionService>();

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(mongoDbSettings.ConnectionString));
builder.Services.AddScoped(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(mongoDbSettings.DatabaseName);
    return database.GetCollection<GameSession>(mongoDbSettings.GameCollectionName);
});

// Add controllers and SignalR services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow the specific frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        policyBuilder.WithOrigins("https://planettest.netlify.app", "http://localhost:5173")
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the application's root
    });
}

// Standard middleware setup
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
