namespace PublicHolidays.Mcp.Server.Holidays;

public sealed record LongWeekendResult(
    string HolidayName,
    string HolidayDate,
    string StartDate,
    string EndDate,
    int ConsecutiveDays,
    int VacationDaysRequired
);

public sealed record BestTimeOffResult(
    string StartDate,
    string EndDate,
    int ConsecutiveDays,
    int VacationDaysRequired,
    string[] VacationDates,
    string[] PublicHolidays
);

public sealed record IsPublicHolidayResult(
    string Date,
    bool IsPublicHoliday,
    string? HolidayName,
    string DayOfWeek
);

public sealed record NextPublicHolidayResult(
    string HolidayDate,
    string HolidayName,
    string DayOfWeek,
    int DaysUntil
);
