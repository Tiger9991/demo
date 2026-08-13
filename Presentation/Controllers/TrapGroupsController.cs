using Application.DTOs;
using Application.Features.TrapGroups.Commands;
using Application.Features.TrapGroups.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrapGroupsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrapGroupsController(IMediator mediator) => _mediator = mediator;

        /// <summary>جلب كل مجموعات المحطات مع إمكانية البحث</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetAllTrapGroupsQuery(search));
            return Ok(result);
        }

        /// <summary>جلب مجموعة محطات بواسطة الـ ID</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetTrapGroupByIdQuery(id));
            if (result is null) return NotFound();
            return Ok(result);
        }

        /// <summary>إنشاء مجموعة محطات جديدة</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TrapGroupUpsertDto data)
        {
            var id = await _mediator.Send(new CreateTrapGroupCommand(data));
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        /// <summary>تعديل بيانات مجموعة محطات</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TrapGroupUpsertDto data)
        {
            data.Id = id;
            var success = await _mediator.Send(new UpdateTrapGroupCommand(data));
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>حذف مجموعة محطات</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _mediator.Send(new DeleteTrapGroupCommand(id));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
