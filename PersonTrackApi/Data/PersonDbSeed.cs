using PersonTrackApi.Models;

namespace PersonTrackApi.Data;

public class PersonDbSeed
{
    public static async Task SeedAsync(PersonContext context)
    {
        if (context.Persons.Count() == 0)
        {
            context.Persons.AddRange(GetFirstContactValues());
            await context.SaveChangesAsync();
        }
    }

    private static List<Person> GetFirstContactValues()
    {
        var contact = new List<Person>()
        {
            new()
            {
                FirstName = "Ferhat",
                LastName = "Köydedurmaz"
            },
            new()
            {
                FirstName = "test",
                LastName = "test"
            },
            new()
            {
                FirstName = "test2",
                LastName = "test2"
            },
        };

        return contact;
    }
}