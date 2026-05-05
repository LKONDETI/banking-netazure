namespace FinCore.Core.Entities.UserAggregate;

/// <summary>
/// Represents a user-assigned alias (nickname) for an account.
/// </summary>
public sealed class Alias
{
    public AliasKey Key { get; }
    public string Name { get; }

    public Alias(AliasKey key, string name)
    {
        ArgumentNullException.ThrowIfNull(key);
        Name = name ?? string.Empty;
        Key = key;
    }
}
