using Application.Features.TapBaitMeasurement.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrapBaitMController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrapBaitMController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CreateTrapBaitMeasurementCommand command) => Ok(await _mediator.Send(command));
    }
}
