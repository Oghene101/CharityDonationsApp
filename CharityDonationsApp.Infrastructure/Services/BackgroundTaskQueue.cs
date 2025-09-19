using System.Threading.Channels;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;

namespace CharityDonationsApp.Infrastructure.Services;

public class BackgroundTaskQueue(int capacity = 100, int timeoutMs = 500) : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue =
        Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(capacity);

    public async ValueTask QueueBackgroundWorkItemAsync(Func<IServiceProvider, CancellationToken, Task> workItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);
        try
        {
            await _queue.Writer.WriteAsync(workItem, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Background task queue is full (capacity {capacity}). Timed out after {timeoutMs}ms.");
        }
    }

    public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}