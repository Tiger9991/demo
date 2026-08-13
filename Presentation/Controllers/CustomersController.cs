using Application.Features.Customers.Commands;
using Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CustomersController(IMediator mediator) => _mediator = mediator;

        /// <summary>جلب كل العملاء مع إمكانية البحث</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetAllCustomersQuery(search));
            return Ok(result);
        }

        /// <summary>جلب عميل بواسطة الـ ID</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id));
            if (result is null) return NotFound();
            return Ok(result);
        }

        /// <summary>جلب محطات العميل عبر مجموعاته</summary>
        [HttpGet("{id:guid}/traps")]
        public async Task<IActionResult> GetTraps(Guid id)
        {
            var result = await _mediator.Send(new GetCustomerTrapsQuery(id));
            return Ok(result);
        }

        /// <summary>إنشاء عميل جديد</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Application.DTOs.CustomerUpsertDto data)
        {
            var id = await _mediator.Send(new CreateCustomerCommand(data));
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        /// <summary>تعديل بيانات عميل</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Application.DTOs.CustomerUpsertDto data)
        {
            data.Id = id;
            var success = await _mediator.Send(new UpdateCustomerCommand(data));
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>حذف عميل</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _mediator.Send(new DeleteCustomerCommand(id));
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>ربط مجموعة محطات بعميل</summary>
        [HttpPost("{customerId:guid}/assign-trap-group/{trapGroupId:guid}")]
        public async Task<IActionResult> AssignTrapGroup(Guid customerId, Guid trapGroupId)
        {
            var success = await _mediator.Send(new AssignTrapGroupToCustomerCommand(customerId, trapGroupId));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
