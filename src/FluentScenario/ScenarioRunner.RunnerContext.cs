namespace DrifterApps.Seeds.FluentScenario;

public sealed partial class ScenarioRunner
{
    /// <inheritdoc/>
    public void SetContextData(string contextKey, object data)
    {
        _ = _context.Remove(contextKey);
        _context.Add(contextKey, data);
    }

    /// <inheritdoc/>
    public T GetContextData<T>(string contextKey) =>
        !_context.TryGetValue(contextKey, out var value)
            ? throw new KeyNotFoundException($"The context data with key {contextKey} was not found.")
            : (T)value;
}
