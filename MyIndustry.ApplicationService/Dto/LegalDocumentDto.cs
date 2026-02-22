using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.ApplicationService.Dto;

public class LegalDocumentDto
{
    public Guid Id { get; set; }
    public LegalDocumentType DocumentType { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string? Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
