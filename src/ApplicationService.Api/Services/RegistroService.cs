using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace ApplicationService.Api.Services
{
    public interface IRegistroService
    {
        Task WriteLogAsync(LogRequest log, CancellationToken cancellationToken);
    }

    public class RegistroService : IRegistroService
    {
        private readonly ILogger _logger;

        public RegistroService(ILogger logger)
        {
            _logger = logger;
        }

        public Task WriteLogAsync(LogRequest log, CancellationToken cancellationToken)
        {   
            try
            {
                _logger.Information("log written in APPLICATION LOG: {0}", JsonSerializer.Serialize(log));
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error: ");
                throw;
            }
        }
    }
}