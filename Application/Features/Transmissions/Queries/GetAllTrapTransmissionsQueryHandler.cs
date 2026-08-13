using Application.Common.Interfaces;
using Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Transmissions.Queries
{
    public class GetAllTrapTransmissionsQueryHandler : IRequestHandler<GetAllTrapTransmissionsQuery, List<TrapTransmissionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllTrapTransmissionsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TrapTransmissionDto>> Handle(GetAllTrapTransmissionsQuery request, CancellationToken cancellationToken)
        {
            var trapsList = await _context.Traps
                .AsNoTracking()
                .OrderBy(t => t.TrapNumber)
                .ToListAsync(cancellationToken);

            return trapsList.Select(t => new TrapTransmissionDto
            {
                TrapId = t.Id,
                TrapNumber = t.TrapNumber,
                NumberOfTransmissions = t.TotalTransmissions,
                OperatingDays = Math.Max(0, (int)(DateTime.UtcNow - t.StartTime).TotalDays)
            }).ToList();
        }
    }
}
