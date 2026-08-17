using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Quartz;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext Factory
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    }));

            // Register ApplicationDbContext (scoped)
            services.AddScoped<ApplicationDbContext>(provider =>
                provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

            // Register IApplicationDbContext (scoped)
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Register IApplicationDbContextFactory
            services.AddScoped<IApplicationDbContextFactory, Infrastructure.Services.ApplicationDbContextFactory>();

            // Generic repository (scoped because DbContext is scoped)
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register Quartz.NET
            services.AddQuartz(q =>
            {
                // Register Jobs
                q.AddJob<Infrastructure.Services.Jobs.BatteryUpdateJob>(opts => opts.WithIdentity("BatteryUpdateJob"));
                q.AddJob<Infrastructure.Services.Jobs.ConnectivityCheckJob>(opts => opts.WithIdentity("ConnectivityCheckJob"));

                // Triggers
                q.AddTrigger(opts => opts
                    .ForJob("BatteryUpdateJob")
                    .WithIdentity("BatteryUpdateTrigger")
                    .WithCronSchedule("0 0 * * * ?")); // Hourly

                q.AddTrigger(opts => opts
                    .ForJob("ConnectivityCheckJob")
                    .WithIdentity("ConnectivityCheckTrigger")
                    .WithCronSchedule("0 */15 * * * ?")); // Every 15 minutes
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            return services;
        }
    }

}
