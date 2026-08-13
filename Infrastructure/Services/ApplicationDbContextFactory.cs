using Application.Common.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class ApplicationDbContextFactory : IApplicationDbContextFactory
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public ApplicationDbContextFactory(IDbContextFactory<ApplicationDbContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken ct = default)
        {
            return await _dbFactory.CreateDbContextAsync(ct);
        }
    }
}
