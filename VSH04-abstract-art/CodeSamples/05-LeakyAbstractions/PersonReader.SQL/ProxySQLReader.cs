using PersonReader.Interface;

namespace PersonReader.SQL;

public class ProxySQLReader : IPersonReader
{
    public async Task<IReadOnlyCollection<Person>> GetPeople()
    {
        using SQLReader reader = new();
        return await reader.GetPeople();
    }

    public async Task<Person?> GetPerson(int id)
    {
        using SQLReader reader = new();
        return await reader.GetPerson(id);
    }
}
