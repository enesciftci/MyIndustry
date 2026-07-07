namespace MyIndustry.Container.Logging;

public static class CorrelationIdContext
{
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

    public static string? Current => CurrentCorrelationId.Value;

    public static IDisposable BeginScope(string correlationId)
    {
        var previous = CurrentCorrelationId.Value;
        CurrentCorrelationId.Value = correlationId;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly string? _previous;

        public Scope(string? previous) => _previous = previous;

        public void Dispose() => CurrentCorrelationId.Value = _previous;
    }
}
