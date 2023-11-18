using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PersonTrackApi.Data;
using PersonTrackApi.Models;

namespace PersonTrackApi.Repositories;

public class PersonRepository(PersonContext context)
{
    private readonly PersonContext _context = context;

    public async Task<Person> GetByIdAsync(int personId)
    {
        var result = await _context.Persons.FindAsync(personId);

        return result;
    }
    public async Task<bool> AddAsync(Person person)
    {
        var result = await _context.Persons.AddAsync(person);

        if (result.State != EntityState.Added)
            return false;

        await _context.SaveChangesAsync();

        return true;
    }
}
