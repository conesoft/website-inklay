namespace Conesoft.Website.Inklay.Services;

public class GarbageCollect(TimeSpan period) : PeriodicTask(period)
{
    protected override async Task Process()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
    }
}
