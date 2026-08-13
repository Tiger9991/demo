using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContextFactory
    {
        Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
    }
}
