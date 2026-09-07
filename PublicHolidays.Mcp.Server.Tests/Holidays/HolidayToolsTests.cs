using PublicHolidays.Mcp.Server.Holidays;
using ModelContextProtocol;
using Moq;
using Xunit;

namespace PublicHolidays.Mcp.Server.Tests.Holidays;

public class HolidayToolsTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static HolidayItem MakeHoliday(string date, string name) =>
        new(date, name, DateOnly.Parse(date).DayOfWeek.ToString(), true, ["Public"]);

    // 2026-01-01 = Thursday
    // 2026-03-15 = Sunday
    // 2026-03-20 = Friday     (fabricated)
    // 2026-04-20 = Monday     (fabricated)
    // 2026-08-20 = Thursday   (fabricated mid-week)
    // 2026-12-25 = Friday     (Christmas, adjacent to 12-26 Saturday)
    // 2026-12-26 = Saturday
    // 2026-12-28 = Monday

    private static HolidayItem[] SampleHolidays2026 => [
        MakeHoliday("2026-01-01", "New Year's Day"),          // Thursday
        MakeHoliday("2026-03-15", "1848 Revolution"),          // Sunday
        MakeHoliday("2026-03-20", "Spring Break"),             // Friday
        MakeHoliday("2026-04-20", "Easter Monday"),            // Monday
        MakeHoliday("2026-08-20", "Constitution Day"),         // Thursday
        MakeHoliday("2026-12-25", "Christmas Day"),            // Friday
        MakeHoliday("2026-12-26", "Second Christmas Day"),     // Saturday
    ];

    private static HolidayTools BuildTools(HolidayItem[]? items = null)
    {
        items ??= SampleHolidays2026;
        var svc = new Mock<INagerHolidaysService>();
        svc.Setup(s => s.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((int year, CancellationToken _) =>
               new HolidaysResponse("HU", year, "Nager.Holidays", items));
        return new HolidayTools(svc.Object);
    }

    // ── find_long_weekends ────────────────────────────────────────────────────

    [Fact]
    public void CalculateLongWeekends_HolidayOnFriday_CreatesFridayToSundayStretch()
    {
        // 2026-03-20 is a Friday
        var holidays = new[] { MakeHoliday("2026-03-20", "Spring Break") };

        var results = HolidayTools.CalculateLongWeekends(2026, holidays);

        var r = Assert.Single(results);
        Assert.Equal("Spring Break", r.HolidayName);
        Assert.Equal("2026-03-20", r.StartDate);   // Friday is the start of the long weekend
        Assert.Equal("2026-03-22", r.EndDate);     // Sunday
        Assert.Equal(3, r.ConsecutiveDays);
        Assert.Equal(0, r.VacationDaysRequired);
    }

    [Fact]
    public void CalculateLongWeekends_HolidayOnMonday_CreatesSaturdayToMondayStretch()
    {
        // 2026-04-20 is a Monday
        var holidays = new[] { MakeHoliday("2026-04-20", "Easter Monday") };

        var results = HolidayTools.CalculateLongWeekends(2026, holidays);

        var r = Assert.Single(results);
        Assert.Equal("Easter Monday", r.HolidayName);
        Assert.Equal("2026-04-18", r.StartDate);   // Saturday
        Assert.Equal("2026-04-20", r.EndDate);     // Monday
        Assert.Equal(3, r.ConsecutiveDays);
        Assert.Equal(0, r.VacationDaysRequired);
    }

    [Fact]
    public void CalculateLongWeekends_HolidayOnSaturday_IsNotLongWeekend()
    {
        // 2026-03-21 is a Saturday — just a normal weekend
        var holidays = new[] { MakeHoliday("2026-03-21", "Sat Holiday") };

        var results = HolidayTools.CalculateLongWeekends(2026, holidays);

        // Sat+Sun = 2 days only, should not appear as a long weekend (< 3 days)
        Assert.Empty(results);
    }

    [Fact]
    public void CalculateLongWeekends_HolidayOnSunday_IsNotLongWeekend()
    {
        // 2026-03-22 is a Sunday
        var holidays = new[] { MakeHoliday("2026-03-22", "Sun Holiday") };

        var results = HolidayTools.CalculateLongWeekends(2026, holidays);

        Assert.Empty(results);
    }

    [Fact]
    public void CalculateLongWeekends_TwoAdjacentHolidays_BothReportedInSameStretch()
    {
        // 2026-12-25 Friday + 2026-12-26 Saturday → stretch is Fri+Sat+Sun = 3 days
        var holidays = new[]
        {
            MakeHoliday("2026-12-25", "Christmas Day"),
            MakeHoliday("2026-12-26", "Second Christmas Day")
        };

        var results = HolidayTools.CalculateLongWeekends(2026, holidays);

        Assert.Equal(2, results.Length);
        Assert.All(results, r =>
        {
            Assert.Equal("2026-12-25", r.StartDate);
            Assert.Equal("2026-12-27", r.EndDate); // Sunday
            Assert.Equal(3, r.ConsecutiveDays);
            Assert.Equal(0, r.VacationDaysRequired);
        });
    }

    // ── find_best_time_off ────────────────────────────────────────────────────

    [Fact]
    public void CalculateBestTimeOff_ZeroVacationDays_OnlyNaturalWeekendStretches()
    {
        // Friday holiday creates Fri+Sat+Sun = 3 days with 0 vacation days
        var holidays = new[] { MakeHoliday("2026-03-20", "Spring Break") }; // Friday

        var results = HolidayTools.CalculateBestTimeOff(2026, holidays, 0);

        Assert.Contains(results, r =>
            r.StartDate == "2026-03-20" && r.EndDate == "2026-03-22" &&
            r.ConsecutiveDays == 3 && r.VacationDaysRequired == 0);
    }

    [Fact]
    public void CalculateBestTimeOff_OneVacationDay_ExtendsFridayHolidayToFourDays()
    {
        // Friday holiday: if we take Thursday as vacation → Thu+Fri+Sat+Sun = 4 days, 1 vacation
        var holidays = new[] { MakeHoliday("2026-03-20", "Spring Break") }; // Friday

        var results = HolidayTools.CalculateBestTimeOff(2026, holidays, 1);

        Assert.Contains(results, r =>
            r.StartDate == "2026-03-19" && r.EndDate == "2026-03-22" &&
            r.ConsecutiveDays == 4 && r.VacationDaysRequired == 1 &&
            r.VacationDates.Contains("2026-03-19")); // Thursday
    }

    [Fact]
    public void CalculateBestTimeOff_TwoVacationDays_ExtendsMondayHolidayToFiveDays()
    {
        // Monday holiday: Sat+Sun+Mon = 3 days. With 2 vacation days (Tue+Wed) → Sat+Sun+Mon+Tue+Wed = 5
        var holidays = new[] { MakeHoliday("2026-04-20", "Easter Monday") }; // Monday

        var results = HolidayTools.CalculateBestTimeOff(2026, holidays, 2);

        Assert.Contains(results, r =>
            r.ConsecutiveDays >= 5 && r.VacationDaysRequired <= 2 &&
            r.PublicHolidays.Any(h => h.Contains("Easter Monday")));
    }

    [Fact]
    public async Task FindBestTimeOffAsync_InvalidMaxVacationDays_ThrowsMcpException()
    {
        var tools = BuildTools();

        var ex = await Assert.ThrowsAsync<McpException>(
            () => tools.FindBestTimeOffAsync(2026, 11));
        Assert.Contains("10", ex.Message);
    }

    [Fact]
    public async Task FindBestTimeOffAsync_NegativeVacationDays_ThrowsMcpException()
    {
        var tools = BuildTools();

        await Assert.ThrowsAsync<McpException>(
            () => tools.FindBestTimeOffAsync(2026, -1));
    }

    // ── is_public_holiday ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsPublicHolidayAsync_OnHolidayDate_ReturnsTrue()
    {
        var tools = BuildTools();

        var result = await tools.IsPublicHolidayAsync("2026-03-20");

        Assert.True(result.IsPublicHoliday);
        Assert.Equal("Spring Break", result.HolidayName);
        Assert.Equal("Friday", result.DayOfWeek);
    }

    [Fact]
    public async Task IsPublicHolidayAsync_OnNonHolidayDate_ReturnsFalse()
    {
        var tools = BuildTools();

        var result = await tools.IsPublicHolidayAsync("2026-03-18"); // Wednesday, no holiday

        Assert.False(result.IsPublicHoliday);
        Assert.Null(result.HolidayName);
    }

    [Fact]
    public async Task IsPublicHolidayAsync_InvalidFormat_ThrowsMcpException()
    {
        var tools = BuildTools();

        var ex = await Assert.ThrowsAsync<McpException>(
            () => tools.IsPublicHolidayAsync("18-03-2026"));
        Assert.Contains("yyyy-MM-dd", ex.Message);
    }

    // ── get_next_public_holiday ───────────────────────────────────────────────

    [Fact]
    public async Task GetNextPublicHolidayAsync_BeforeFirstHoliday_ReturnsFirstHoliday()
    {
        var tools = BuildTools();

        var result = await tools.GetNextPublicHolidayAsync("2026-01-01");

        Assert.Equal("2026-01-01", result.HolidayDate);
        Assert.Equal("New Year's Day", result.HolidayName);
        Assert.Equal(0, result.DaysUntil);
    }

    [Fact]
    public async Task GetNextPublicHolidayAsync_MidYear_ReturnsNextUpcoming()
    {
        var tools = BuildTools();

        var result = await tools.GetNextPublicHolidayAsync("2026-03-16");

        // After 2026-03-15, next is 2026-03-20 in SampleHolidays2026
        Assert.Equal("2026-03-20", result.HolidayDate);
        Assert.Equal(4, result.DaysUntil);
    }

    [Fact]
    public async Task GetNextPublicHolidayAsync_InvalidDate_ThrowsMcpException()
    {
        var tools = BuildTools();

        await Assert.ThrowsAsync<McpException>(
            () => tools.GetNextPublicHolidayAsync("not-a-date"));
    }

    // ── year validation ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public async Task FindLongWeekendsAsync_InvalidYear_ThrowsMcpException(int year)
    {
        var tools = BuildTools();

        await Assert.ThrowsAsync<McpException>(
            () => tools.FindLongWeekendsAsync(year));
    }

    [Fact]
    public void CalculateBestTimeOff_NoHolidaysNearWeekend_NoResults()
    {
        // Mid-week holiday only (Thursday), maxVacation=0 → no 3+ day stretch possible
        var holidays = new[] { MakeHoliday("2026-08-20", "Constitution Day") }; // Thursday

        var results = HolidayTools.CalculateBestTimeOff(2026, holidays, 0);

        // Thursday alone is 1 day; no contiguous stretch with weekend possible with 0 vacation days
        Assert.DoesNotContain(results, r => r.VacationDaysRequired == 0 && r.ConsecutiveDays >= 3
            && r.PublicHolidays.Any(h => h.Contains("Constitution Day")));
    }
}
