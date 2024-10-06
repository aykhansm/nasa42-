using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using CelestialOrrery.Hubs;
using CelestialOrrery.Utils;  // If you have utility classes or configurations
using CelestialOrrery.Models;
using CelestialOrrery.Services;  // Add this line


var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
var mongoDbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<MongoDBSettings>();
if (mongoDbSettings == null)
    throw new InvalidOperationException("Database settings are not configured properly.");

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
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Or use JsonNamingPolicy.CamelCase
    options.JsonSerializerOptions.WriteIndented = true; // Makes the JSON output readable, useful in development
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

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // To serve the Swagger UI at the application's root
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
