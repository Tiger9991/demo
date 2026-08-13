using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Stats.Queries
{
    public record GetLatestAlertTimeQuery : IRequest<LatestAlertTimeDto>;
}
