using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TrapGroups.Commands
{
    public sealed class DeleteTrapGroupCommandHandler
        : IRequestHandler<DeleteTrapGroupCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteTrapGroupCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            DeleteTrapGroupCommand request,
            CancellationToken cancellationToken)
        {
            var group = await _context.TrapGroups
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (group is null) return false;

            _context.TrapGroups.Remove(group);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
