using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var rabbitMqHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));
builder.Services.AddHttpClient<ISellerClient, SellerClient>(client =>
{
    client.BaseAddress = new Uri("http://sellerservice:8080/");
});
builder.Services.AddHostedService<OrderCreatedConsumer>();
builder.Services.AddHostedService<OrderCancelledConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

