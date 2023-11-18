using System.Data.SqlTypes;

namespace PersonTrackApi.Models;

public class PersonMovementSearchKey
{
    public int PersonId { get; set; }
    public DateTime DateStart { get; set; } = SqlDateTime.MinValue.Value;
    public DateTime DateEnd { get; set; } = SqlDateTime.MaxValue.Value;
}
