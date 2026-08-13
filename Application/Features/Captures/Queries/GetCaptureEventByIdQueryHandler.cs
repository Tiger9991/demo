using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Captures.Queries
{
    public class GetCaptureEventByIdQueryHandler
      : IRequestHandler<GetCaptureEventByIdQuery, CaptureEventDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCaptureEventByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CaptureEventDto> Handle(
            GetCaptureEventByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Load the capture event with its related Trap
            var capture = await _context.CaptureEvents
                .Include(c => c.Trap)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (capture == null)
                throw new Exception($"CaptureEvent with ID '{request.Id}' not found.");

            // Map to DTO
            return new CaptureEventDto
            {
               
              
               
            };
        }
    }
}
