namespace MyIndustry.Container.Logging;

[AttributeUsage(AttributeTargets.Property)]
public sealed class LogSensitiveAttribute : Attribute
{
    public LogMaskMode Mode { get; init; } = LogMaskMode.Redact;
    public int MaxLength { get; init; } = 256;
}
