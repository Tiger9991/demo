using Application.Features.Captures.Commands;
using Application.Features.TapBaitMeasurement.Commands;
using Application.Features.Traps.Commands;
using Application.Features.Traps.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;


namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class TrapsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrapsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null)
        {
            return Ok(await _mediator.Send(new GetAllTrapsQuery(customerId)));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _mediator.Send(new GetTrapByIdQuery(id)));
        }

        [HttpPost(Name = "Init_Master_Slave_Trap")]
        [SwaggerOperation(OperationId = "init_master_salve_trap")]
        public async Task<IActionResult> Create([FromBody] CreateTrapCommand command) => Ok(await _mediator.Send(command));

        [HttpPost("{id}/capture")]
        public async Task<IActionResult> RecordCapture(Guid id, [FromBody] RecordCaptureCommand command)
        {
           
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalTraps([FromQuery] string? groupNumber, [FromQuery] string? status, [FromQuery] Guid? customerId = null)
        {
            var result = await _mediator.Send(new GetTotalTrapsQuery(groupNumber, status, customerId));
            return Ok(result);
        }

        [HttpGet("inactive-count")]
        public async Task<IActionResult> GetInactiveCount()
        {
            var count = await _mediator.Send(new GetInactiveTrapsCountQuery());
            return Ok(count);
        }

        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactiveTraps()
        {
            var result = await _mediator.Send(new GetInactiveTrapsQuery());
            return Ok(result);
        }

        [HttpGet("low-battery-detail")]
        public async Task<IActionResult> GetLowBatteryDetail(
            [FromQuery] int? threshold = 30,
            [FromQuery] string? groupNumber = null)
        {
            var result = await _mediator.Send(
                new GetLowBatteryTrapsDetailQuery(threshold ?? 30, groupNumber));
            return Ok(result);
        }

        [HttpGet("low-battery-count")]
        public async Task<IActionResult> GetLowBatteryCount(
            [FromQuery] int? threshold = 30,
            [FromQuery] string? status = "Active",
            [FromQuery] string? groupNumber = null)
        {
            var count = await _mediator.Send(
                new GetLowBatteryCountQuery(threshold ?? 30, status, groupNumber));
            return Ok(count);
        }

        [HttpGet("needing-refill-count")]
        public async Task<IActionResult> GetTrapsNeedingRefillCount(
            [FromQuery] double? threshold = 50,
            [FromQuery] string? status = "Active",
            [FromQuery] string? groupNumber = null)
        {
            var result = await _mediator.Send(new GetTrapsNeedingRefillCountQuery(
                threshold ?? 50,
                status,
                groupNumber));
            return Ok(result);
        }

        [HttpGet("needing-refill-details")]
        public async Task<IActionResult> GetTrapsNeedingRefillDetails(
            [FromQuery] double? threshold = 50,
            [FromQuery] string? status = "Active",
            [FromQuery] string? groupNumber = null)
        {
            var result = await _mediator.Send(new GetTrapsNeedingRefillQuery(
                threshold ?? 50,
                status,
                groupNumber));
            return Ok(result);
        }

        
    }
}
