using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetRodentActivityQuery(
    string? GroupNumber = null,
    string? Status = null,          // defaults to "Active" if not provided
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<RodentActivityDto>;
}
