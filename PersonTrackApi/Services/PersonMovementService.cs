using PersonTrackApi.Models;
using PersonTrackApi.Repositories;
using PersonTrackApi.Utitilies.Enums;
using PersonTrackApi.Utitilies.Results;
using RabbitMQEventBus.Constants;
using RabbitMQEventBus.Events;
using RabbitMQEventBus.Producer;
using System.Linq.Expressions;

namespace PersonTrackApi.Services;

public class PersonMovementService
{
    private readonly PersonMovementRepository _personMovementRepository;
    private readonly PersonService _personService;
    private readonly PersonMovementReportService _personMovementReportService;
    private readonly IRabbitMQEventBusProducer _eventBus;

    public PersonMovementService(PersonMovementRepository personMovementRepository, PersonService personService, IRabbitMQEventBusProducer eventBus, PersonMovementReportService personMovementReportService)
    {
        _personMovementRepository = personMovementRepository;
        _personService = personService;
        _eventBus = eventBus;
        _personMovementReportService = personMovementReportService;
    }

    public async Task<BaseResponse> AddPersonEnterMovementAsync(int personId)
    {
        try
        {
            var personLastProcess = await _personMovementRepository.GetPersonLastProcess(personId);

            if (personLastProcess is not null && personLastProcess.MovementType == (int)MovementTypeEnum.Enter)
                return new BaseResponse(false, "Person already entered");

            PersonMovementCreateEvent personMovementCreateEvent = new()
            {
                PersonId = personId,
                MovementType = (int)MovementTypeEnum.Enter,
                MovementTypeName = nameof(MovementTypeEnum.Enter),
                CreatedAt = DateTime.UtcNow
            };

            var result = await AddPersonMovementAsync(personMovementCreateEvent);

            return result;
        }
        catch
        {
            return new BaseResponse(false, "Failed to add person movement");
        }
    }

    public async Task<BaseResponse> AddPersonExitMovementAsync(int personId)
    {
        try
        {
            var personLastProcess = await _personMovementRepository.GetPersonLastProcess(personId);

            if (personLastProcess is not null && personLastProcess.MovementType == (int)MovementTypeEnum.Exit)
                return new BaseResponse(false, "Person already exited");

            PersonMovementCreateEvent personMovementCreateEvent = new()
            {
                PersonId = personId,
                MovementType = (int)MovementTypeEnum.Exit,
                MovementTypeName = nameof(MovementTypeEnum.Exit),
                CreatedAt = DateTime.UtcNow
            };

            var result = await AddPersonMovementAsync(personMovementCreateEvent);

            return result;
        }
        catch
        {
            return new BaseResponse(false, "Failed to add person movement");
        }
    }

    public async Task<BaseResponse> AddPersonMovementProcess(PersonMovement personMovement)
    {
        try
        {
            var result = await _personMovementRepository.AddAsync(personMovement);

            if (result == false)
                return new BaseResponse(false, "Failed to add person movement");

            _ = await _personMovementReportService.AddPersonMovementForReportAsync(personMovement);

            return new BaseResponse(true, "Person movement added successfully");
        }
        catch
        {
            return new BaseResponse(false, "Failed to add person movement");
        }
    }
    public async Task<BaseDataResponse<PersonMovement>> GetPersonMovementById(int id)
    {
        try
        {
            var result = await _personMovementRepository.GetById(id);

            if (result is null)
                return new BaseDataResponse<PersonMovement>(default, false, "Movement not found");

            return new BaseDataResponse<PersonMovement>(result, true);
        }
        catch
        {
            return new BaseDataResponse<PersonMovement>(default, false, "Failed to get movement");
        }
    }
    public async Task<BaseDataResponse<List<PersonMovement>>> GetPersonMovementsWithQuery(PersonMovementSearchKey searchKey)
    {
        try
        {
            searchKey.DateStart = searchKey.DateStart.Date.ToUniversalTime();
            searchKey.DateEnd = searchKey.DateEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();

            Expression<Func<PersonMovement, bool>> filterExpression;
            if (searchKey.PersonId > 0)

                filterExpression = e => e.PersonId == searchKey.PersonId && e.CreatedAt >= searchKey.DateStart && e.CreatedAt <= searchKey.DateEnd;
            else
                filterExpression = e => e.CreatedAt >= searchKey.DateStart && e.CreatedAt <= searchKey.DateEnd;

            var result = await _personMovementRepository.GetPersonMovementWithQuery(filterExpression);

            return new BaseDataResponse<List<PersonMovement>>(result, true);
        }
        catch
        {
            return new BaseDataResponse<List<PersonMovement>>(default, false, "Failed to get person movements");
        }
    }
    private async Task<BaseResponse> AddPersonMovementAsync(PersonMovementCreateEvent personMovementCreate)
    {
        var person = await _personService.GetPersonAsync(personMovementCreate.PersonId);

        if (person.Success == false)
            return new BaseResponse(false, "Person not found");

        _eventBus.Publish(EventConstants.PersonMovementReportQueue, personMovementCreate);

        return new BaseResponse(true, "Person movement added successfully");
    }
}
