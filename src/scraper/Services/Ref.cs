namespace Scraper.Services;

/// <summary>
/// A reference type that can be used to wrap value types so they can be injected into 
/// <see cref="Microsoft.Extensions.DependencyInjection.KeyedServiceCollectionExtensions"/>.
/// </summary>
/// <typeparam name="T">The type being wrapped.</typeparam>
public class Ref<T>
{
    public T Value { get; set; }

    public Ref(T value)
    {
        Value = value;
    }
}