using PersonTrackApi.Models;
using PersonTrackApi.Repositories;
using PersonTrackApi.Utitilies.Enums;
using PersonTrackApi.Utitilies.Results;

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

            if(result == false)
                return new BaseResponse(false, "Failed to update person movement for report");

            return new BaseResponse(true, "Person movement for report updated successfully");
        }
    }

    public async Task<BaseDataResponse<List<PersonMovementReport>>> GetPersonMovementReportWithQuery(PersonMovementSearchKey searchKeys)
    {
        searchKeys.DateStart = searchKeys.DateStart.Date.ToUniversalTime();
        searchKeys.DateEnd = searchKeys.DateEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();

        var result = await _personMovementReportRepository.GetPersonMovementReportWithQuery(searchKeys);

        return new BaseDataResponse<List<PersonMovementReport>>(result,true);
    }
}