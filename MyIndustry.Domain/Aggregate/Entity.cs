using System.ComponentModel.DataAnnotations;

namespace MyIndustry.Domain.Aggregate;

public class Entity : IEntity
{
    [Key]
    public virtual Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    public void SetId(Guid id)
    {
        Id = id;
    }
}

public interface IEntity
{
    Guid Id { get; set; }
    DateTime CreatedDate { get; set; }
    DateTime? ModifiedDate { get; set; }
}