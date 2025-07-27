namespace Conesoft.Website.Inklay.Tools;

public static class LinqHelperExtensions
{
    public static IEnumerable<IEnumerable<T>> GroupWhile<T>(this IEnumerable<T> seq, Func<T, T, bool> condition)
    {
        T prev = seq.First();
        List<T> list = [prev];

        foreach (T item in seq.Skip(1))
        {
            if (condition(prev, item) == false)
            {
                yield return list;
                list = [];
            }
            list.Add(item);
            prev = item;
        }

        yield return list;
    }
}
