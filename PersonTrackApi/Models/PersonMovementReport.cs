using System.ComponentModel.DataAnnotations.Schema;

namespace PersonTrackApi.Models;

public class PersonMovementReport
{
    public int Id { get; set; }
    [ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public DateTime EnterDate { get; set; }
    public DateTime ExitDate { get; set; }
    public TimeSpan TimeDifference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
