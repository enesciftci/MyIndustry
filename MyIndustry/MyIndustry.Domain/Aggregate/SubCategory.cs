namespace MyIndustry.Domain.Aggregate;

public class SubCategory : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public ICollection<Service> Services { get; set; }
    public Category Category { get; set; }
}