using Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TrapGroups.Commands
{
    public sealed class CreateTrapGroupCommandHandler
        : IRequestHandler<CreateTrapGroupCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateTrapGroupCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(
            CreateTrapGroupCommand request,
            CancellationToken cancellationToken)
        {
            var group = new Domain.Entities.TrapGroups
            {
                Id = Guid.NewGuid(),
                TrapNumber = request.Data.TrapNumber.Trim(),
                TrapGroup = request.Data.TrapGroup.Trim(),
                Description = request.Data.Description?.Trim(),
                CustomerId = request.Data.CustomerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TrapGroups.Add(group);
            await _context.SaveChangesAsync(cancellationToken);

            return group.Id;
        }
    }
}
