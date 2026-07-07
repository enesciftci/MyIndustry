using System.ComponentModel.DataAnnotations;
using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.ApplicationService.Dto;

public class LegalDocumentDto
{
    public Guid Id { get; set; }
    public LegalDocumentType DocumentType { get; set; }

    [StringLength(500)]
    public string Title { get; set; } = "";

    [StringLength(50_000)]
    public string Content { get; set; } = "";

    [StringLength(50)]
    public string? Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
