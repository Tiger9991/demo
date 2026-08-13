using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Transmissions.Queries
{
    public record GetAllTrapTransmissionsQuery : IRequest<List<TrapTransmissionDto>>;
}
