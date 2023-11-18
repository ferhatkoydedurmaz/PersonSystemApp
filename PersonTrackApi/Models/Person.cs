using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace PersonTrackApi.Models;

public class Person
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Admin kullanıcısı olmadığı için varsayılan 1 veriyoruz.
    public int CreatedBy { get; set; } = 1;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdateBy { get; set; }
    public ICollection<PersonMovement> PersonTrackReports { get; } = new List<PersonMovement>();
    public ICollection<PersonMovementReport> PersonMovementReports { get; } = new List<PersonMovementReport>();
}
