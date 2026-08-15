namespace BoxingGame.Domain.Strikes;

// Single lookup from key name to Strike instance.
// Add new entries here when new strikes are added — engine is untouched.
public static class StrikeCatalog
{
    private static readonly Dictionary<string, Strike> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Head-targeting strikes
        [LeftJab.Instance.Name]        = LeftJab.Instance,
        [RightJab.Instance.Name]       = RightJab.Instance,
        [LeftHook.Instance.Name]       = LeftHook.Instance,
        [RightHook.Instance.Name]      = RightHook.Instance,
        [LeftUppercut.Instance.Name]   = LeftUppercut.Instance,
        [RightUppercut.Instance.Name]  = RightUppercut.Instance,
        // Body-targeting strikes (activated when body-modifier key is held)
        [BodyLeftJab.Instance.Name]    = BodyLeftJab.Instance,
        [BodyRightJab.Instance.Name]   = BodyRightJab.Instance,
        [BodyLeftHook.Instance.Name]   = BodyLeftHook.Instance,
        [BodyRightHook.Instance.Name]  = BodyRightHook.Instance,
    };

    public static Strike? Get(string name) =>
        _map.TryGetValue(name, out var s) ? s : null;
}
