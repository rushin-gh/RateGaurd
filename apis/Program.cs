using apis.Business;
using apis.Contracts;
using apis.Middleware;
using apis.Model;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RetryWindowSettings>(
    builder.Configuration.GetSection("RetryWindow")
);

builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
//app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<RedisRateLimitMiddleware>();
app.MapControllers();

app.Run();
