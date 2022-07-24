using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using W4k.AspNetCore.Correlator.Context;

namespace W4k.AspNetCore.Correlator.Example.NetCore60.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly ILogger<HelloController> _logger;

        public HelloController(ICorrelationContextAccessor contextAccessor, ILogger<HelloController> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Hello()
        {
            _logger.LogInformation("Entering hello");

            var correlationId = _contextAccessor.CorrelationContext.CorrelationId;
            var result = correlationId.IsEmpty
                ? "<correlation missing>"
                : correlationId;

            _logger.LogInformation("Request almost finished, correlation: {correlationId}", correlationId);

            return Ok(result);
        }
    }
}
