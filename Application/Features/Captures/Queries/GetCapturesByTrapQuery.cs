using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Captures.Queries
{
    public record GetCapturesByTrapQuery(
    string TrapNumber,
    string? GroupNumber = null   // 👈 This property is required
) : IRequest<List<CaptureEventDto>>;
}
