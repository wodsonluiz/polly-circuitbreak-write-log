using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly.CircuitBreaker;
using Serilog;

namespace ApplicationClient.Api.Services
{
    public interface IRegistroService
    {
        Task WriteLogAsync(LogRequest log, CancellationToken cancellationToken);
    }

    public class RegistroService : IRegistroService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public RegistroService(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("LogService");
        }

        public async Task WriteLogAsync(LogRequest log, CancellationToken cancellationToken)
        {   
            try
            {
                var objLog = JsonSerializer.Serialize(log);

                var content = new StringContent(objLog,
                    Encoding.UTF8,
                    "application/json");

                _logger.Information("log written in CLIENT: {0}", objLog);

                var response = await _httpClient.PostAsync("api/v1/registros", content, cancellationToken);
        
                // Verificar se a chamada foi bem-sucedida
                response.EnsureSuccessStatusCode();
            }
            catch (BrokenCircuitException)
            {
                await Task.CompletedTask;
            }
        }
    }
}