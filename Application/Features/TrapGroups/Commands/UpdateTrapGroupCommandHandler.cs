using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TrapGroups.Commands
{
    public sealed class UpdateTrapGroupCommandHandler
        : IRequestHandler<UpdateTrapGroupCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTrapGroupCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            UpdateTrapGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Data.Id is null) return false;

            var group = await _context.TrapGroups
                .FirstOrDefaultAsync(g => g.Id == request.Data.Id, cancellationToken);

            if (group is null) return false;

            group.TrapNumber = request.Data.TrapNumber.Trim();
            group.TrapGroup = request.Data.TrapGroup.Trim();
            group.Description = request.Data.Description?.Trim();
            group.CustomerId = request.Data.CustomerId;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
