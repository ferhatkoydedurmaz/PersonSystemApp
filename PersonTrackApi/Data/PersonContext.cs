using Microsoft.EntityFrameworkCore;
using PersonTrackApi.Models;

namespace PersonTrackApi.Data;

public class PersonContext(DbContextOptions<PersonContext> options) : DbContext(options)
{
    public DbSet<Person> Persons { get; set; }
    public DbSet<PersonMovement> PersonMovements { get; set; }
    public DbSet<PersonMovementReport> PersonMovementReports { get; set; }
}
