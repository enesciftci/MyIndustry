using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Domain.Aggregate;

public class LegalDocument : Entity
{
    public LegalDocumentType DocumentType { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string? Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int DisplayOrder { get; set; }
}
