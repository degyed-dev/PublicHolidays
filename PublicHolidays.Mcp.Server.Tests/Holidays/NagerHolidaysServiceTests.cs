using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using PublicHolidays.Mcp.Server.Holidays;
using ModelContextProtocol;
using Xunit;

namespace PublicHolidays.Mcp.Server.Tests.Holidays;

public class NagerHolidaysServiceTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    private static readonly NagerHolidayDto[] SampleDtos =
    [
        new("2026-01-01", "Újév", "New Year's Day", "HU", true, true, null, null, ["Public"]),
        new("2026-03-15", "Nemzeti ünnep", "1848 Revolution Memorial Day", "HU", true, true, null, null, ["Public"])
    ];

    private static IHttpClientFactory BuildFactory(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://nagerholidays.com"),
            Timeout     = TimeSpan.FromSeconds(10)
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("nager")).Returns(client);
        return factory.Object;
    }

    private static IMemoryCache BuildCache() =>
        new MemoryCache(new MemoryCacheOptions());

    private static HttpResponseMessage OkResponse() =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(SampleDtos)
        };

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ValidYear_ReturnsNormalizedResponse()
    {
        var svc = new NagerHolidaysService(BuildFactory(OkResponse()), BuildCache());

        var result = await svc.GetAsync(2026);

        Assert.Equal("HU", result.CountryCode);
        Assert.Equal(2026, result.Year);
        Assert.Equal("Nager.Holidays", result.Source);
        Assert.Equal(2, result.Holidays.Length);

        var first = result.Holidays[0];
        Assert.Equal("2026-01-01", first.Date);
        Assert.Equal("New Year's Day", first.Name);
        Assert.Equal("Thursday", first.DayOfWeek);
        Assert.True(first.NationalHoliday);
        Assert.Contains("Public", first.HolidayTypes);
    }

    [Theory]
    [InlineData("1999")]
    [InlineData("2101")]
    [InlineData("abc")]
    [InlineData("")]
    public async Task GetHungarianHolidaysAsync_InvalidYear_ThrowsMcpException(string year)
    {
        var resource = new HolidaysResource(Mock.Of<INagerHolidaysService>());

        await Assert.ThrowsAsync<McpException>(
            () => resource.GetHungarianHolidaysAsync(year));
    }

    [Fact]
    public async Task GetAsync_UpstreamHttpError_ThrowsMcpException()
    {
        var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var svc = new NagerHolidaysService(BuildFactory(errorResponse), BuildCache());

        var ex = await Assert.ThrowsAsync<McpException>(() => svc.GetAsync(2026));

        Assert.DoesNotContain("nagerholidays.com", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("500", ex.Message);
        // Message should be user-friendly but not expose internal URL
        Assert.Contains("2026", ex.Message);
    }

    [Fact]
    public async Task GetAsync_SecondCallSameYear_HitsCache()
    {
        var callCount = 0;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                return OkResponse();
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("https://nagerholidays.com") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("nager")).Returns(client);

        var cache = BuildCache();
        var svc = new NagerHolidaysService(factory.Object, cache);

        await svc.GetAsync(2026);
        await svc.GetAsync(2026);

        Assert.Equal(1, callCount);
    }
}
