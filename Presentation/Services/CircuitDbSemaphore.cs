using System.Threading;

namespace Presentation.Services
{
    public class CircuitDbSemaphore
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
    }
}
