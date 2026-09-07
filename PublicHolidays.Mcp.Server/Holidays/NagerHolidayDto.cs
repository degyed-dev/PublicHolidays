using System.Text.Json.Serialization;

namespace PublicHolidays.Mcp.Server.Holidays;

internal sealed record NagerHolidayDto(
    [property: JsonPropertyName("date")]        string Date,
    [property: JsonPropertyName("localName")]   string LocalName,
    [property: JsonPropertyName("name")]        string Name,
    [property: JsonPropertyName("countryCode")] string CountryCode,
    [property: JsonPropertyName("fixed")]       bool Fixed,
    [property: JsonPropertyName("global")]      bool Global,
    [property: JsonPropertyName("counties")]    string[]? Counties,
    [property: JsonPropertyName("launchYear")]  int? LaunchYear,
    [property: JsonPropertyName("types")]       string[]? Types
);
