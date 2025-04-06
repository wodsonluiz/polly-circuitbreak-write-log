
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System;
using System.IO;

namespace ApplicationClient.Api
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLogConfiguration(this IServiceCollection services)
        {
            var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logFilePath = Path.Combine(logFolder, "log-client-.txt");

            Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.Debug()
                .WriteTo.Console(formatter: new ColoredFormatter())
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day, // Gera um arquivo novo por dia
                    retainedFileCountLimit: 7            // Mantém os últimos 7 arquivos
                )
                .CreateLogger();

            services.AddSingleton(Log.Logger);

            return services;
        }

        public static IServiceCollection AddPollyAndHttpClient(this IServiceCollection services)
        {
            // Defina a política de Circuit Breaker
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // Lida com HttpRequestException, 5xx e 408 (timeout)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 1, // Número de falhas antes de abrir o circuito
                    durationOfBreak: TimeSpan.FromSeconds(10), // Tempo que o circuito permanece aberto
                    onBreak: (result, timeSpan) =>
                    {
                        // Log quando o circuito abrir
                        Log.Logger.Warning($"Circuit breaker opened for {timeSpan.TotalSeconds} seconds due to: {result.Exception?.Message}");
                    },
                    onReset: () =>
                    {
                        // Log quando o circuito fechar novamente
                        Log.Logger.Information("Circuit breaker reset.");
                    },
                    onHalfOpen: () =>
                    {
                        // Log quando o circuito estiver em half-open (testando se deve fechar)
                        Log.Logger.Warning("Circuit breaker is half-open, testing if system is healthy.");
                    });

            // Configurar o HttpClient com a política
            services.AddHttpClient("LogService", client =>
            {
                //Check the port used by the logging application
                client.BaseAddress = new Uri("http://localhost:5201");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(circuitBreakerPolicy);

            return services;
        }
    }
}

