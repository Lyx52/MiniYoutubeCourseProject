using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class IdEntity<TKey>
{
    [Key]
    public TKey Id { get; set; }
}