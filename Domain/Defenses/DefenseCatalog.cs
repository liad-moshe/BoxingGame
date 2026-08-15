namespace BoxingGame.Domain.Defenses;

// Single lookup from key name to Defense instance.
public static class DefenseCatalog
{
    private static readonly Dictionary<string, Defense> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        [BothHandsBlock.Instance.Name] = BothHandsBlock.Instance,
        [Duck.Instance.Name]           = Duck.Instance,
    };

    public static Defense? Get(string name) =>
        _map.TryGetValue(name, out var d) ? d : null;
}
