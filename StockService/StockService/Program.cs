using Microsoft.EntityFrameworkCore;
using StockService;
using StockService.Bus;
using StockService.Eventos;
using StockService.Models;
using StockService.services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEntityFrameworkSqlServer()
.AddDbContext<SagaPatternLabContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("sagaConnection"));
});

// Agregar modelo de configuracion
builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMQ"));

// Agregar modelo de configuracion
RabbitMqConfiguration rbConf = new RabbitMqConfiguration
{
    HostName = builder.Configuration["RabbitMq:HostName"],
    Username = builder.Configuration["RabbitMq:Username"],
    Password = builder.Configuration["RabbitMq:Password"],
    Port = int.Parse(builder.Configuration["RabbitMq:Port"])
};

// RABBIT MQ CONFIGURACION
builder.Services.AddSingleton<RabbitMqConfiguration>(rbConf);
builder.Services.AddSingleton<EventBus>();

// Registrar interfaz
builder.Services.AddScoped<IOrderRepository, StockServices>();
builder.Services.AddScoped<StockServices>();

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

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var eventBus = serviceProvider.GetRequiredService<EventBus>();

    // Suscribirnos a eventos del otro microservicio
    eventBus.Subscribe<OrderCreatedEvent>("order_exchange", "stock_service_order_created_queue", "order.created", serviceProvider.GetRequiredService<StockServices>().HandleOrderCreatedEvent);
   // eventBus.Subscribe<OrderCancelledEvent>("order_exchange", "stock_service_order_cancelled_queue", "order.cancelled", serviceProvider.GetRequiredService<StockServices().HandleOrderCancelledEvent);
}

app.Run();
