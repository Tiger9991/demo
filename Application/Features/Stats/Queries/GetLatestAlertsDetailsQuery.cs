using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetLatestAlertsDetailsQuery(int Count = 10) : IRequest<List<LatestAlertDetailDto>>;
}
