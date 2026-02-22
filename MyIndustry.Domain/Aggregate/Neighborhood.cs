namespace MyIndustry.Domain.Aggregate;

/// <summary>
/// Mahalleler
/// </summary>
public class Neighborhood : Entity
{
    public string Name { get; set; }
    public Guid DistrictId { get; set; }
    public bool IsActive { get; set; } = true;
    
    public District District { get; set; }
}
