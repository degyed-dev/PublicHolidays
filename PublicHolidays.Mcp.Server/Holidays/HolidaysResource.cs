using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace PublicHolidays.Mcp.Server.Holidays;

[McpServerResourceType]
public sealed class HolidaysResource(INagerHolidaysService service)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented               = false
    };

    [McpServerResource(
        UriTemplate = "holidays://HU/{year}",
        Name        = "hungarian-public-holidays",
        Title       = "Hungarian Public Holidays",
        MimeType    = "application/json")]
    [Description(
        "Returns Hungarian public holidays for the requested year from Nager.Holidays. " +
        "Year must be an integer between 2000 and 2100.")]
    public async Task<TextResourceContents> GetHungarianHolidaysAsync(
        string year,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(year, out var yearInt) || yearInt < 2000 || yearInt > 2100)
            throw new McpException(
                $"Year must be an integer between 2000 and 2100 (got '{year}').");

        var response = await service.GetAsync(yearInt, cancellationToken);

        return new TextResourceContents
        {
            Uri      = $"holidays://HU/{year}",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(response, SerializerOptions)
        };
    }
}
