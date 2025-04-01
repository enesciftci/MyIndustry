namespace MyIndustry.Domain.Aggregate;

public class Category : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public ICollection<SubCategory> SubCategories { get; set; }
}