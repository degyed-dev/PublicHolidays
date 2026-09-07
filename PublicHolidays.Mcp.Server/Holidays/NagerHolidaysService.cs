using Microsoft.Extensions.Caching.Memory;
using ModelContextProtocol;
using System.Net.Http.Json;

namespace PublicHolidays.Mcp.Server.Holidays;

public interface INagerHolidaysService
{
    Task<HolidaysResponse> GetAsync(int year, CancellationToken cancellationToken = default);
}

internal sealed class NagerHolidaysService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
    : INagerHolidaysService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<HolidaysResponse> GetAsync(int year, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"holidays_HU_{year}";
        if (cache.TryGetValue(cacheKey, out HolidaysResponse? cached) && cached is not null)
            return cached;

        var client = httpClientFactory.CreateClient("nager");

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync($"/api/v4/Holidays/HU/{year}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new McpException("Unable to reach the Nager.Holidays service. Please try again later.");
        }
        catch (TaskCanceledException)
        {
            throw new McpException("The request to Nager.Holidays timed out. Please try again later.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new McpException(
                $"Nager.Holidays returned an error for year {year}. Please try again later.");
        }

        NagerHolidayDto[]? dtos;
        try
        {
            dtos = await response.Content.ReadFromJsonAsync<NagerHolidayDto[]>(cancellationToken);
        }
        catch (Exception)
        {
            throw new McpException("Received an unrecognized response from Nager.Holidays.");
        }

        if (dtos is null)
            throw new McpException($"Nager.Holidays returned no data for year {year}.");

        var items = dtos.Select(dto => new HolidayItem(
            Date:            dto.Date,
            Name:            dto.Name,
            DayOfWeek:       DateOnly.Parse(dto.Date).DayOfWeek.ToString(),
            NationalHoliday: dto.Global,
            HolidayTypes:    dto.Types ?? []
        )).ToArray();

        var result = new HolidaysResponse("HU", year, "Nager.Holidays", items);

        cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        });

        return result;
    }
}
