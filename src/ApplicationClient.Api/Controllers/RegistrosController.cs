using System.Threading;
using System.Threading.Tasks;
using ApplicationClient.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationClient.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RegistrosController : ControllerBase
    {
        private readonly IRegistroService _service;

        public RegistrosController(IRegistroService service) =>
            _service = service;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LogRequest logRequest, CancellationToken cancellationToken)
        {
            await _service.WriteLogAsync(logRequest, cancellationToken);
            return StatusCode(201);
        }
    }
}