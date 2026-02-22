namespace MyIndustry.Domain.Aggregate;

/// <summary>
/// Türkiye'deki iller
/// </summary>
public class City : Entity
{
    public string Name { get; set; }
    public int PlateCode { get; set; } // Plaka kodu (01-81)
    public bool IsActive { get; set; } = true;
    
    public ICollection<District> Districts { get; set; } = new List<District>();
}
