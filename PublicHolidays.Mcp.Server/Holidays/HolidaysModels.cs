namespace PublicHolidays.Mcp.Server.Holidays;

public sealed record HolidayItem(
    string Date,
    string Name,
    string DayOfWeek,
    bool NationalHoliday,
    string[] HolidayTypes
);

public sealed record HolidaysResponse(
    string CountryCode,
    int Year,
    string Source,
    HolidayItem[] Holidays
);
