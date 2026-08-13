using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.BaitMeasurement.Commands;
using Application.Features.BaitMeasurement.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;   // 👈 Add thi

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaitMeasurementsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BaitMeasurementsController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBaitMeasurementById(Guid id)
        {
            var result = await _mediator.Send(new GetBaitMeasurementByIdQuery(id));
            return Ok(result);
        }

        // other actions...
    }
}
