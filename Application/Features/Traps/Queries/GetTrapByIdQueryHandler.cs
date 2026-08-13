using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Queries
{
    public class GetTrapByIdQueryHandler : IRequestHandler<GetTrapByIdQuery, TrapDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetTrapByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TrapDto> Handle(GetTrapByIdQuery request, CancellationToken cancellationToken)
        {
            var trap = await _context.Traps
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (trap == null)
                throw new NotFoundException(nameof(Trap), request.Id);

            return _mapper.Map<TrapDto>(trap);
        }
    }
}
