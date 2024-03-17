using DotNet.RateLimiter;
using DotNet.RateLimiter.Demo;
using DotNet.RateLimiter.Extensions;

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

app.MapGet("/weatherforecast", () =>
{
    return Results.Ok("Hi I'm here!");
})
.WithName("GetWeatherForecast")
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 2;
});

app.Run();
