namespace Conesoft.Website.Inklay.Services;

public abstract class PeriodicTask
{
    private readonly PeriodicTimer timer;

    public PeriodicTask(TimeSpan period)
    {
        timer = new(period);
        var _ = StartPeriodicCacheUpdate();
    }

    protected abstract Task Process();

    public async Task StartPeriodicCacheUpdate()
    {
        do
        {
            await Process();
        }
        while (await timer.WaitForNextTickAsync());
    }
}
