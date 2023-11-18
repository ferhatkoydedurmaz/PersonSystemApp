using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using PersonTrackApi.Consumer;
using PersonTrackApi.Data;
using PersonTrackApi.Repositories;
using PersonTrackApi.Services;
using PersonTrackApi.Utitilies.Extensions;
using Polly;
using RabbitMQ.Client;
using RabbitMQEventBus;
using RabbitMQEventBus.Producer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<PersonContext>(sql =>
{
    sql.UseNpgsql(builder.Configuration.GetConnectionString("defaultConnStr"));
}, ServiceLifetime.Singleton, ServiceLifetime.Singleton);

//RabbitMQ
builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["EventBus:HostName"]
    };

    if (!string.IsNullOrEmpty(builder.Configuration["EventBus:UserName"]))
    {
        factory.UserName = builder.Configuration["EventBus:UserName"];
    }

    if (!string.IsNullOrEmpty(builder.Configuration["EventBus:Password"]))
    {
        factory.Password = builder.Configuration["EventBus:Password"];
    }

    var retryCount = 5;
    if (!string.IsNullOrEmpty(builder.Configuration["EventBus:RetryCount"]))
    {
        if (int.TryParse(builder.Configuration["EventBus:RetryCount"], out var retryCountt))
            retryCount = retryCountt;
    }

    return new DefaultRabbitMQPersistentConnection(factory, retryCount, logger);
}
);
builder.Services.AddSingleton<IRabbitMQEventBusProducer, RabbitMQEventBusProducer>();
builder.Services.AddSingleton<PersonMovementReportEventBusConsumer>();

builder.Services.AddSingleton<PersonMovementReportRepository>();
builder.Services.AddSingleton<PersonMovementRepository>();
builder.Services.AddSingleton<PersonRepository>();

builder.Services.AddSingleton<PersonService>();
builder.Services.AddSingleton<PersonMovementService>();
builder.Services.AddSingleton<PersonMovementReportService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseEventBusListener();

#region database oluþturma
using var scope = app.Services.CreateScope();

var hasMigration = scope.ServiceProvider.GetRequiredService<PersonContext>().Database.GetPendingMigrations().Any();
if (hasMigration == true)
{
    var context = scope.ServiceProvider.GetRequiredService<PersonContext>();
    context.Database.Migrate();
    PersonDbSeed.SeedAsync(context).Wait();
}
#endregion

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
