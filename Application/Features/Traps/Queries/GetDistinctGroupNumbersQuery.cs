using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public record GetDistinctGroupNumbersQuery : IRequest<List<string>>;
}
