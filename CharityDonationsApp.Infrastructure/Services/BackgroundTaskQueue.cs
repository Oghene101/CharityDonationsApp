using System.Threading.Channels;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;

namespace CharityDonationsApp.Infrastructure.Services;

public class BackgroundTaskQueue(int capacity = 100) : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue =
        Channel.CreateBounded<Func<CancellationToken, Task>>(capacity);

    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        if (!_queue.Writer.TryWrite(workItem)) throw new InvalidOperationException("Queue is full");
    }

    public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}