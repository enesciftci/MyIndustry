namespace MyIndustry.Container.Logging;

public class MediatRLoggingOptions
{
    public const string SectionName = "MediatRLogging";

    public bool Enabled { get; set; } = true;
    public bool LogRequestPayload { get; set; } = true;
    public int MaxStringLength { get; set; } = 256;
    public bool MaskEmails { get; set; } = true;
    public bool MaskPhones { get; set; } = true;
}
