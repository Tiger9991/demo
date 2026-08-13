using Application.Features.TapBaitMeasurement.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrapBaitMeasurementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrapBaitMeasurementController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("TrapBaitM")]

        public async Task<IActionResult> CreateTrapBait([FromBody] CreateTrapBaitMeasurementCommand command) => Ok(await _mediator.Send(command));
    }
}
