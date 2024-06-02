namespace Conesoft.Website.Inklay.Tools;

public static class ResolverExtension
{
    // Just a helper method to shorten code registration code
    // based on
    // https://stackoverflow.com/a/53885374/1528847

    public static IServiceCollection AddSingletonWith<T>(this IServiceCollection collection, params object[] parameters) where T : class
    {
        return collection.AddSingleton(c => ActivatorUtilities.CreateInstance<T>(c, parameters));
    }
}