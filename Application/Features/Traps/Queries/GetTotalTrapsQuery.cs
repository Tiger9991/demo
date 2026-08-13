using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetTotalTrapsQuery(string? GroupNumber = null, string? Status = null, Guid? CustomerId = null) : IRequest<TrapsTotalDto>;
}
