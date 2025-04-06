using System;
using System.IO;
using ApplicationService.Api;
using ApplicationService.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRegistroService, RegistroService>();

// Configure log
var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

if(!Directory.Exists(logFolder))
{
    Directory.CreateDirectory(logFolder);
}

var logFilePath = Path.Combine(logFolder, "log-client-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(formatter: new ColoredFormatter())
    .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Day, // Gera um arquivo novo por dia
        retainedFileCountLimit: 7            // Mantém os últimos 7 arquivos
    )
    .CreateLogger();

builder.Services.AddSingleton(Log.Logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/api/v1/health");
app.MapControllers();

app.Run();