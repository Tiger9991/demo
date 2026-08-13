using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.BaitMeasurement.Commands
{
    public class DeleteBaitMeasurementCommandHandler : IRequestHandler<DeleteBaitMeasurementCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteBaitMeasurementCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteBaitMeasurementCommand request, CancellationToken cancellationToken)
        {
            var measurement = await _context.BaitMeasurements
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (measurement == null)
                throw new NotFoundException(nameof(BaitMeasurement), request.Id);

            _context.BaitMeasurements.Remove(measurement);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
