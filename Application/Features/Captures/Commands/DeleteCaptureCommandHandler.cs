using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Captures.Commands
{
    public class DeleteCaptureCommandHandler : IRequestHandler<DeleteCaptureCommand>
    {
        private readonly IApplicationDbContext _context;
        public DeleteCaptureCommandHandler(IApplicationDbContext context) => _context = context;
        public async Task Handle(DeleteCaptureCommand request, CancellationToken ct)
        {
            var capture = await _context.CaptureEvents.FindAsync(new object[] { request.Id }, ct);
            if (capture == null) throw new NotFoundException(nameof(CaptureEvent), request.Id);
            _context.CaptureEvents.Remove(capture);
            await _context.SaveChangesAsync(ct);
        }
    }

}
