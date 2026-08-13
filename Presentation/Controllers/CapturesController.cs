using Application.DTOs;
using Application.Features.Captures.Commands;
using Application.Features.Captures.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CapturesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CapturesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Record a new capture event.
        /// </summary>
        /// <param name="command">The capture data.</param>
        /// <returns>The created capture event.</returns>
        [HttpPost]
        public async Task<ActionResult<string>> RecordCapture([FromBody] RecordCaptureCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get all capture events for a specific trap.
        /// </summary>
        /// <param name="trapNumber">The trap number.</param>
        /// <param name="groupNumber">Optional group number for validation.</param>
        /// <returns>List of capture events.</returns>
        [HttpGet("by-trap")]
        public async Task<ActionResult<List<CaptureEventDto>>> GetCapturesByTrap(
            [FromQuery] string trapNumber,
            [FromQuery] string? groupNumber = null)
        {
            var result = await _mediator.Send(new GetCapturesByTrapQuery(trapNumber, groupNumber));
            return Ok(result);
        }

        /// <summary>
        /// Get a single capture event by its ID.
        /// </summary>
        /// <param name="id">The capture event ID.</param>
        /// <returns>The capture event.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CaptureEventDto>> GetCaptureEventById(Guid id)
        {
            var result = await _mediator.Send(new GetCaptureEventByIdQuery(id));
            return Ok(result);
        }
    }
}
