using Microsoft.EntityFrameworkCore;
using PersonTrackApi.Data;
using PersonTrackApi.Models;
using System;
using System.Linq.Expressions;

namespace PersonTrackApi.Repositories;

public class PersonMovementRepository(PersonContext context)
{
    private readonly PersonContext _context = context;

    public async Task<bool> AddAsync(PersonMovement personMovement)
    {
        var result = await _context.PersonMovements.AddAsync(personMovement);

        if (result.State != EntityState.Added)
            return false;

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<PersonMovement> GetPersonLastProcess(int personId)
    {
        var result = await _context.PersonMovements.Where(w => w.PersonId == personId).OrderByDescending(d=>d.Id).FirstOrDefaultAsync();

        return result;
    }

    public async Task<PersonMovement> GetById(int movementId)
    {
        var result = await _context.PersonMovements.FindAsync(movementId);

        return result;
    }

    public async Task<List<PersonMovement>> GetPersonMovementWithQuery(Expression<Func<PersonMovement,bool>> expression)
    {
        var result = await _context.PersonMovements.Where(expression).ToListAsync();
        //var result = await _context.PersonMovements.Where(w => w.PersonId == searchKeys.PersonId && w.CreatedAt >= searchKeys.DateStart && w.CreatedAt <= searchKeys.DateEnd).ToListAsync();

        return result;
    }
}
