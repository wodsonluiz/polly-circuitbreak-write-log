using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationService.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ApplicationService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RegistrosController : ControllerBase
    {
        private readonly IRegistroService _service;

        private static DateTime _cycleStartTime = DateTime.UtcNow;
        private const int ERROR_DURATION_SECONDS = 5;
        private const int SUCCESS_DURATION_SECONDS = 60;
        private const int TOTAL_CYCLE_SECONDS = ERROR_DURATION_SECONDS + SUCCESS_DURATION_SECONDS;

        private readonly ILogger _logger;

        public RegistrosController(IRegistroService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }
            

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LogRequest logRequest, CancellationToken cancellationToken)
        {
            // Calcula quanto tempo se passou desde o início do ciclo atual
            TimeSpan elapsedTime = DateTime.UtcNow - _cycleStartTime;
            
            // Se o tempo do ciclo atual ultrapassou a duração total do ciclo, reiniciamos a contagem
            if (elapsedTime.TotalSeconds >= TOTAL_CYCLE_SECONDS)
            {
                _cycleStartTime = DateTime.UtcNow;
                elapsedTime = TimeSpan.Zero;
            }

            // Se estamos nos primeiros 5 segundos do ciclo, retorne erro 500
            if (elapsedTime.TotalSeconds < ERROR_DURATION_SECONDS)
            {
                _logger.Error("Scheduled error every 5 seconds - {0}", DateTime.UtcNow);
                return StatusCode(500);
            }

            await _service.WriteLogAsync(logRequest, cancellationToken);
            return StatusCode(201);
        }
    }
}