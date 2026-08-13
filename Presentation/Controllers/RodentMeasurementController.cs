using Application.Features.Rodent.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RodentMeasurementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RodentMeasurementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Calculate rodent length and type based on active IR sensors and provided weight.
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculateRodentMeasurementQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
