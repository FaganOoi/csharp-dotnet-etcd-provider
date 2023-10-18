using DemoProject.Models;
using DotnetEtcdProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEtcdConfiguration("Etcd");

builder.Services.Configure<AppSettingsFromEtcd>(builder.Configuration.GetSection("AppSettingsFromEtcd"));
builder.Services.Configure<Dev2>(builder.Configuration.GetSection("Dev2"));


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

app.Run();
