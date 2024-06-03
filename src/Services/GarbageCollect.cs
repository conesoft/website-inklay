using Humanizer;
using Serilog;

namespace Conesoft.Website.Inklay.Services;

public class GarbageCollect(TimeSpan period) : PeriodicTask(period)
{
    protected override Task Process()
    {
        var memoryBefore = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        var memoryAfter = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
        Log.Information($"running garbage collection cycle: {Math.Max(0, memoryBefore - memoryAfter).Bytes()} released, {memoryAfter.Bytes()} in use");

        return Task.CompletedTask;
    }
}
