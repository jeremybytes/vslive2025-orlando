using PersonReader.CSV;
using PersonReader.Interface;
using PersonReader.Service;
using PersonReader.SQL;

namespace PeopleViewer;

public class ReaderFactory
{
    public static IPersonReader GetReader(string readerType)
    {
        return readerType switch
        {
            "Service" => new ServiceReader(),
            "CSV" => new CSVReader(),
            "SQL" => new ProxySQLReader(),
            _ => throw new ArgumentException("Invalid reader type"),
        };
    }
}
