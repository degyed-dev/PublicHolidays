using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace PublicHolidays.Mcp.Server.Holidays;

[McpServerToolType]
public sealed class HolidayTools(INagerHolidaysService holidayService)
{
    private const int MinYear = 2000;
    private const int MaxYear = 2100;
    private const int MaxAllowedVacationDays = 10;

    // ── public MCP tools ──────────────────────────────────────────────────────

    [McpServerTool(Name = "find_long_weekends")]
    [Description(
        "Finds Hungarian public holidays that naturally create a long weekend of 3 or more " +
        "consecutive days off without requiring any vacation days. A long weekend occurs when a " +
        "holiday falls on a Friday or Monday, or when a holiday is adjacent to the weekend or " +
        "to another public holiday. Returns each holiday with the full stretch of days off it " +
        "belongs to.")]
    public async Task<LongWeekendResult[]> FindLongWeekendsAsync(
        [Description("The calendar year to analyse (2000–2100).")] int year,
        CancellationToken cancellationToken = default)
    {
        ValidateYear(year);
        var response = await holidayService.GetAsync(year, cancellationToken);
        return CalculateLongWeekends(year, response.Holidays);
    }

    [McpServerTool(Name = "find_best_time_off")]
    [Description(
        "Finds the best opportunities to create long continuous breaks in Hungary by combining " +
        "weekends, public holidays, and a limited number of vacation days. Results are ranked " +
        "by total consecutive days off (highest first), then by vacation days required (lowest " +
        "first), then by earliest start date. Use this to plan the most efficient use of " +
        "vacation days around Hungarian public holidays.")]
    public async Task<BestTimeOffResult[]> FindBestTimeOffAsync(
        [Description("The calendar year to analyse (2000–2100).")] int year,
        [Description("Maximum number of vacation days available to spend (0–10).")] int maxVacationDays,
        CancellationToken cancellationToken = default)
    {
        ValidateYear(year);
        if (maxVacationDays < 0 || maxVacationDays > MaxAllowedVacationDays)
            throw new McpException(
                $"maxVacationDays must be between 0 and {MaxAllowedVacationDays} (got {maxVacationDays}).");

        var response = await holidayService.GetAsync(year, cancellationToken);
        return CalculateBestTimeOff(year, response.Holidays, maxVacationDays);
    }

    [McpServerTool(Name = "is_public_holiday")]
    [Description(
        "Checks whether a specific date is a Hungarian public holiday. Returns the holiday " +
        "name if the date is a holiday, along with the day of the week.")]
    public async Task<IsPublicHolidayResult> IsPublicHolidayAsync(
        [Description("The date to check, in ISO 8601 format (yyyy-MM-dd).")] string date,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var d))
            throw new McpException($"Invalid date '{date}'. Expected format: yyyy-MM-dd.");

        ValidateYear(d.Year);
        var response = await holidayService.GetAsync(d.Year, cancellationToken);

        var holiday = response.Holidays.FirstOrDefault(h => h.Date == date);
        return new IsPublicHolidayResult(
            Date:          date,
            IsPublicHoliday: holiday is not null,
            HolidayName:   holiday?.Name,
            DayOfWeek:     d.DayOfWeek.ToString()
        );
    }

    [McpServerTool(Name = "get_next_public_holiday")]
    [Description(
        "Returns the next Hungarian public holiday on or after the given date. If no holiday " +
        "remains in the current year, automatically searches the following year.")]
    public async Task<NextPublicHolidayResult> GetNextPublicHolidayAsync(
        [Description("The start date for the search, in ISO 8601 format (yyyy-MM-dd).")] string fromDate,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParseExact(fromDate, "yyyy-MM-dd", out var from))
            throw new McpException($"Invalid fromDate '{fromDate}'. Expected format: yyyy-MM-dd.");

        ValidateYear(from.Year);

        // Search this year, then next year if needed
        for (var year = from.Year; year <= from.Year + 1 && year <= MaxYear; year++)
        {
            var response = await holidayService.GetAsync(year, cancellationToken);
            var next = response.Holidays
                .Select(h => (Item: h, Date: DateOnly.Parse(h.Date)))
                .Where(x => x.Date >= from)
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            if (next.Item is not null)
            {
                return new NextPublicHolidayResult(
                    HolidayDate: next.Item.Date,
                    HolidayName: next.Item.Name,
                    DayOfWeek:   next.Date.DayOfWeek.ToString(),
                    DaysUntil:   next.Date.DayNumber - from.DayNumber
                );
            }
        }

        throw new McpException($"No Hungarian public holiday found from {fromDate} within the supported year range.");
    }

    // ── internal calculation logic (static for testability) ──────────────────

    internal static LongWeekendResult[] CalculateLongWeekends(int year, HolidayItem[] holidays)
    {
        var holidayMap = holidays.ToDictionary(h => DateOnly.Parse(h.Date), h => h.Name);
        var freeDays   = BuildFreeDays(year, holidayMap.Keys);
        var stretches  = FindContiguousStretches(freeDays);

        var results = new List<LongWeekendResult>();
        foreach (var (start, end) in stretches)
        {
            int days = end.DayNumber - start.DayNumber + 1;
            if (days < 3) continue;

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (holidayMap.TryGetValue(d, out var name))
                {
                    results.Add(new LongWeekendResult(
                        HolidayName:          name,
                        HolidayDate:          d.ToString("yyyy-MM-dd"),
                        StartDate:            start.ToString("yyyy-MM-dd"),
                        EndDate:              end.ToString("yyyy-MM-dd"),
                        ConsecutiveDays:      days,
                        VacationDaysRequired: 0
                    ));
                }
            }
        }

        return [.. results.OrderBy(r => r.HolidayDate)];
    }

    internal static BestTimeOffResult[] CalculateBestTimeOff(
        int year, HolidayItem[] holidays, int maxVacationDays)
    {
        var holidayMap = holidays.ToDictionary(h => DateOnly.Parse(h.Date), h => h.Name);
        var freeDays   = BuildFreeDays(year, holidayMap.Keys);

        var yearStart = new DateOnly(year, 1, 1);
        var yearEnd   = new DateOnly(year, 12, 31);

        var candidates = new List<BestTimeOffResult>();

        for (var start = yearStart; start <= yearEnd; start = start.AddDays(1))
        {
            var vacDates  = new List<DateOnly>();
            bool hasFree  = false;
            var current   = start;

            while (current <= yearEnd)
            {
                bool isFree = freeDays.Contains(current);
                // Stop before adding a work day that would exceed the vacation budget
                if (!isFree && vacDates.Count >= maxVacationDays) break;

                if (isFree) hasFree = true;
                else vacDates.Add(current);

                current = current.AddDays(1);
            }

            var end = current.AddDays(-1);
            if (end < start) continue;

            int totalDays = end.DayNumber - start.DayNumber + 1;
            if (totalDays < 3 || !hasFree) continue;

            var involvedHolidays = Enumerable.Range(0, totalDays)
                .Select(i => start.AddDays(i))
                .Where(holidayMap.ContainsKey)
                .Select(d => $"{d:yyyy-MM-dd}: {holidayMap[d]}")
                .ToArray();

            if (involvedHolidays.Length == 0) continue; // only weekends — not interesting

            candidates.Add(new BestTimeOffResult(
                StartDate:           start.ToString("yyyy-MM-dd"),
                EndDate:             end.ToString("yyyy-MM-dd"),
                ConsecutiveDays:     totalDays,
                VacationDaysRequired: vacDates.Count,
                VacationDates:       [.. vacDates.Select(d => d.ToString("yyyy-MM-dd"))],
                PublicHolidays:      involvedHolidays
            ));
        }

        // Remove dominated results: A is dominated by B when B contains A and uses ≤ vacation days
        var nonDominated = candidates
            .Where(a => !candidates.Any(b =>
                b.StartDate.CompareTo(a.StartDate) <= 0 &&
                b.EndDate.CompareTo(a.EndDate) >= 0 &&
                b.ConsecutiveDays > a.ConsecutiveDays &&
                b.VacationDaysRequired <= a.VacationDaysRequired))
            .DistinctBy(r => (r.StartDate, r.EndDate))
            .OrderByDescending(r => r.ConsecutiveDays)
            .ThenBy(r => r.VacationDaysRequired)
            .ThenBy(r => r.StartDate)
            .ToArray();

        return nonDominated;
    }

    // ── private helpers ───────────────────────────────────────────────────────

    private static HashSet<DateOnly> BuildFreeDays(int year, IEnumerable<DateOnly> holidayDates)
    {
        var free = new HashSet<DateOnly>(holidayDates);
        var d    = new DateOnly(year, 1, 1);
        var end  = new DateOnly(year, 12, 31);
        while (d <= end)
        {
            if (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                free.Add(d);
            d = d.AddDays(1);
        }
        return free;
    }

    private static List<(DateOnly Start, DateOnly End)> FindContiguousStretches(
        HashSet<DateOnly> freeDays)
    {
        var sorted = freeDays.Order().ToList();
        if (sorted.Count == 0) return [];

        var result = new List<(DateOnly, DateOnly)>();
        var start  = sorted[0];
        var prev   = sorted[0];

        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].DayNumber == prev.DayNumber + 1)
            {
                prev = sorted[i];
            }
            else
            {
                result.Add((start, prev));
                start = sorted[i];
                prev  = sorted[i];
            }
        }
        result.Add((start, prev));
        return result;
    }

    private static void ValidateYear(int year)
    {
        if (year < MinYear || year > MaxYear)
            throw new McpException(
                $"Year must be between {MinYear} and {MaxYear} (got {year}).");
    }
}
