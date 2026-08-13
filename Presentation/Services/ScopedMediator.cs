using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Presentation.Services
{
    public class ScopedMediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public ScopedMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            return await mediator.Send(request, cancellationToken);
        }

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            await mediator.Send(request, cancellationToken);
        }

        public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            return await mediator.Send(request, cancellationToken);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            await mediator.Publish(notification, cancellationToken);
        }

        public async Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            await mediator.Publish(notification, cancellationToken);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            return mediator.CreateStream(request, cancellationToken);
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();
            return mediator.CreateStream(request, cancellationToken);
        }
    }
}
