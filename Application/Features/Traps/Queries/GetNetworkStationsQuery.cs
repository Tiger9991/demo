using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Traps.Queries
{
    public record GetNetworkStationsQuery(Guid? CustomerId = null) : IRequest<List<NetworkStationDto>>;
}
