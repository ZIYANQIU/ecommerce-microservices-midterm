using Microsoft.EntityFrameworkCore;
using SellerService.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SellerDbContext>(options =>
    options.UseSqlite("Data Source=sellers.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SellerDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

