using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Captures.Queries
{
    public record GetCaptureEventByIdQuery(Guid Id) : IRequest<CaptureEventDto>;
}
