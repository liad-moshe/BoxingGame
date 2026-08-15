using System.Collections.Concurrent;

namespace BoxingGame.Domain.Match;

public class MatchRegistry
{
    private readonly ConcurrentDictionary<string, Match> _matches = new(StringComparer.OrdinalIgnoreCase);

    public Match CreateMatch()
    {
        string code;
        Match match;
        do
        {
            code  = GenerateCode();
            match = new Match(code);
        } while (!_matches.TryAdd(code, match));
        return match;
    }

    /// <summary>Creates a match where both players share one browser tab.</summary>
    public Match CreateLocalMatch(bool p1Southpaw = false, bool p2Southpaw = false)
    {
        string code;
        Match match;
        do
        {
            code  = GenerateCode();
            match = new Match(code);
        } while (!_matches.TryAdd(code, match));
        match.StartLocalMatch(p1Southpaw, p2Southpaw);
        return match;
    }

    public Match? GetMatch(string code) =>
        _matches.TryGetValue(code, out var m) ? m : null;

    public IEnumerable<Match> AllMatches() => _matches.Values;

    public void RemoveFinished()
    {
        foreach (var (key, match) in _matches)
            if (match.Phase == MatchPhase.Finished)
                _matches.TryRemove(key, out _);
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return string.Create(6, chars, static (span, c) =>
        {
            var rng = Random.Shared;
            for (int i = 0; i < span.Length; i++)
                span[i] = c[rng.Next(c.Length)];
        });
    }
}
