using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetAllTrapsQuery(Guid? CustomerId = null) : IRequest<List<TrapDto>>;

}
