using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonTrackApi.Models;

public class PersonMovement
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public int MovementType { get; set; } //Enter = 1, Exit = 2
    public string MovementTypeName { get; set; } //Enter = 1, Exit = 2
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
