using DotNet.RateLimiter;
using DotNet.RateLimiter.Demo;
using DotNet.RateLimiter.Extensions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRateLimitService(builder.Configuration);


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

app.MapGet("/weatherforecast/{id}/{name}", (int id, string name, [FromQuery] string search, [FromQuery] int? age) =>
{
    return Results.Ok($"Hi I'm here! {id} - {name} - {search} - {age}");
})
.WithName("GetWeatherForecast")
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 2;
});

app.Run();
