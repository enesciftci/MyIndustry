namespace MyIndustry.Domain.Aggregate;

public class Category : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = new();
    public ICollection<Service> Services { get; set; }
}