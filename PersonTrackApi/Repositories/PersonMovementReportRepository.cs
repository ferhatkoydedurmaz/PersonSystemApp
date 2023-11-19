using Microsoft.EntityFrameworkCore;
using PersonTrackApi.Data;
using PersonTrackApi.Models;
using System.Linq.Expressions;

namespace PersonTrackApi.Repositories;

public class PersonMovementReportRepository(PersonContext context)
{
    private readonly PersonContext _context = context;

    public async Task<PersonMovementReport> GetTodayRecordByPersonId(int personId)
    {

        DateTime today = DateTime.UtcNow;
        DateTime startOfDay = today.Date;
        DateTime endOfDay = today.Date.AddDays(1).AddTicks(-1);

        var result = await _context.PersonMovementReports.SingleOrDefaultAsync(s => s.PersonId == personId && s.CreatedAt >= startOfDay && s.CreatedAt <= endOfDay);

        return result;
    }

    public async Task<bool> AddAsync(PersonMovementReport personMovementReport)
    {
        var result = await _context.PersonMovementReports.AddAsync(personMovementReport);

        if (result.State != EntityState.Added)
            return false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateExitDateAsync(PersonMovementReport personMovementReport)
    {
        var result = _context.PersonMovementReports.Update(personMovementReport);

        if (result.State != EntityState.Modified)
            return false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<PersonMovementReport>> GetPersonMovementReportWithQuery(Expression<Func<PersonMovementReport, bool>> filterExpression)
    {
        var result = await _context.PersonMovementReports.Where(filterExpression).ToListAsync();

        return result;
    }
}
