using System.Collections.Generic;

namespace Phork.Blazor.Lifecycle;

internal static class LifecycleExtensions
{
    public static void NotifyCycleEndedAndRemoveDisposedElements<T>(this ICollection<T> collection)
        where T : ILifecycleElement
    {
        var inactiveElements = new List<T>();

        foreach (var element in collection)
        {
            element.NotifyCycleEnded();

            if (element.IsDisposed)
            {
                inactiveElements.Add(element);
            }
        }

        foreach (var item in inactiveElements)
        {
            collection.Remove(item);
        }
    }

    public static void NotifyCycleEndedAndRemoveDisposedElements<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        where TValue : ILifecycleElement
    {
        foreach (var item in dictionary)
        {
            item.Value.NotifyCycleEnded();

            if (item.Value.IsDisposed)
            {
                dictionary.Remove(item.Key);
            }
        }
    }
}