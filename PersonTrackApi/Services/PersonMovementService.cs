using PersonTrackApi.Models;
using PersonTrackApi.Repositories;
using PersonTrackApi.Utitilies.Enums;
using PersonTrackApi.Utitilies.Results;
using RabbitMQEventBus.Constants;
using RabbitMQEventBus.Events;
using RabbitMQEventBus.Producer;

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

    public async Task<BaseResponse> AddPersonExitMovementAsync(int personId)
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

    public async Task<BaseResponse> AddPersonMovementProcess(PersonMovement personMovement)
    {
        var result = await _personMovementRepository.AddAsync(personMovement);

        if (result == false)
            return new BaseResponse(false, "Failed to add person movement");

        _ = await _personMovementReportService.AddPersonMovementForReportAsync(personMovement);

        return new BaseResponse(true, "Person movement added successfully");
    }
    public async Task<BaseDataResponse<PersonMovement>> GetPersonMovementById(int id)
    {
        var result = await _personMovementRepository.GetById(id);

        if(result is null)
            return new BaseDataResponse<PersonMovement>(default, false, "Movement not found");

        return new BaseDataResponse<PersonMovement>(result, true);
    }
    public async Task<BaseDataResponse<List<PersonMovement>>> GetPersonMovementsWithQuery(PersonMovementSearchKey searchKey)
    {
        searchKey.DateStart = searchKey.DateStart.Date.ToUniversalTime();
        searchKey.DateEnd = searchKey.DateEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();

        var result = await _personMovementRepository.GetPersonMovementWithQuery(searchKey);

        return new BaseDataResponse<List<PersonMovement>>(result, true);
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
