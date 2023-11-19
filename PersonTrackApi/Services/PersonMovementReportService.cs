using PersonTrackApi.Models;
using PersonTrackApi.Repositories;
using PersonTrackApi.Utitilies.Enums;
using PersonTrackApi.Utitilies.Results;
using System.Linq.Expressions;

namespace PersonTrackApi.Services;

public class PersonMovementReportService
{
    private readonly PersonMovementReportRepository _personMovementReportRepository;

    public PersonMovementReportService(PersonMovementReportRepository personMovementReportRepository)
    {
        _personMovementReportRepository = personMovementReportRepository;
    }

    public async Task<BaseResponse> AddPersonMovementForReportAsync(PersonMovement personMovement)
    {
        try
        {
            var todayReport = await _personMovementReportRepository.GetTodayRecordByPersonId(personMovement.PersonId);

            if (todayReport is null)
            {
                if (personMovement.MovementType == (int)MovementTypeEnum.Exit)
                    return new BaseResponse(false, "The person is not logged in yet");

                PersonMovementReport personMovementReport = new()
                {
                    PersonId = personMovement.PersonId,
                    EnterDate = DateTime.UtcNow,
                };
                var result = await _personMovementReportRepository.AddAsync(personMovementReport);

                if (result == false)
                    return new BaseResponse(false, "Failed to add person movement for report");

                return new BaseResponse(true, "Person movement for report added successfully");
            }
            else
            {
                if (personMovement.MovementType == (int)MovementTypeEnum.Enter)
                    return new BaseResponse(true);

                todayReport.ExitDate = DateTime.UtcNow;
                todayReport.TimeDifference = todayReport.ExitDate - todayReport.EnterDate;
                todayReport.UpdatedAt = DateTime.UtcNow;
                var result = await _personMovementReportRepository.UpdateExitDateAsync(todayReport);

                if (result == false)
                    return new BaseResponse(false, "Failed to update person movement for report");

                return new BaseResponse(true, "Person movement for report updated successfully");
            }
        }
        catch
        {
            return new BaseResponse(false, "Failed to add person movement for report");
        }
    }

    public async Task<BaseDataResponse<List<PersonMovementReport>>> GetPersonMovementReportWithQuery(PersonMovementSearchKey searchKey)
    {
        try
        {
            searchKey.DateStart = searchKey.DateStart.Date.ToUniversalTime();
            searchKey.DateEnd = searchKey.DateEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();

            Expression<Func<PersonMovementReport, bool>> filterExpression;

            if (searchKey.PersonId > 0)
                filterExpression = e => e.PersonId == searchKey.PersonId && e.CreatedAt >= searchKey.DateStart && e.CreatedAt <= searchKey.DateEnd;
            else
                filterExpression = e => e.CreatedAt >= searchKey.DateStart && e.CreatedAt <= searchKey.DateEnd;

            var result = await _personMovementReportRepository.GetPersonMovementReportWithQuery(filterExpression);

            return new BaseDataResponse<List<PersonMovementReport>>(result, true);
        }
        catch 
        {
            return new BaseDataResponse<List<PersonMovementReport>>(default, false, "Failed to get person movement report");
        }
    }
}