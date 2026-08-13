using Application.Features.Battery.Commands;
using Application.Features.Battery.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/battery")]
    public class BatteryCalculationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BatteryCalculationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Calculate battery status using trap number and (optionally) a custom transmission count.
        /// If transmissionsCount is not provided, uses the actual stored value from the trap.
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateBattery([FromBody] CalculateBatteryFromTransmissionsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get the current and calculated battery status for a trap by its ID.
        /// Does not persist any changes.
        /// </summary>
        [HttpGet("{id:guid}/status")]
        public async Task<IActionResult> GetBatteryStatus(Guid id)
        {
            var result = await _mediator.Send(new GetBatteryStatusQuery(id));
            return Ok(result);
        }

        /// <summary>
        /// Recalculate and persist the battery percentage for a trap by its ID.
        /// Returns the previous and newly calculated battery values.
        /// </summary>
        [HttpPost("{id:guid}/recalculate")]
        public async Task<IActionResult> RecalculateBattery(Guid id)
        {
            var result = await _mediator.Send(new RecalculateBatteryCommand(id));
            return Ok(result);
        }
    }
}
