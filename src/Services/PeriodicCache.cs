namespace Conesoft.Website.Inklay.Services;

public abstract class PeriodicCache<T>(TimeSpan period) : PeriodicTask(period) where T : class
{
    private T? content = null;

    public async Task<T> GetContent()
    {
        while(content == null)
        {
            await Task.Delay(25);
        }
        return content;
    }

    protected override async Task Process() => content = await Generate();

    protected abstract Task<T> Generate();
}
