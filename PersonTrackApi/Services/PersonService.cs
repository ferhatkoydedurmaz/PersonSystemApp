using PersonTrackApi.Models;
using PersonTrackApi.Repositories;
using PersonTrackApi.Utitilies.Results;

namespace PersonTrackApi.Services;

public class PersonService
{
    private readonly PersonRepository _personRepository;

    public PersonService(PersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<BaseDataResponse<Person>> GetPersonAsync(int personId)
    {
        try
        {

            var result = await _personRepository.GetByIdAsync(personId);

            if (result is null)
                return new BaseDataResponse<Person>(default, false, "Person not found");

            return new BaseDataResponse<Person>(result, true);
        }
        catch
        {
            return new BaseDataResponse<Person>(default, false, "Failed to get person");
        }
    }
}
