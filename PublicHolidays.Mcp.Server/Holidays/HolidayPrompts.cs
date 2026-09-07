using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace PublicHolidays.Mcp.Server.Holidays;

[McpServerPromptType]
public sealed class HolidayPrompts
{
    [McpServerPrompt(Name = "best_time_off")]
    [Description(
        "Generates a prompt for finding the best times to take a break in Hungary by combining " +
        "weekends, public holidays, and a limited number of vacation days.")]
    public static GetPromptResult BestTimeOff(
        [Description("The calendar year to analyse (2000–2100).")] int year,
        [Description("Maximum number of vacation days available to spend (0–10).")] int maxVacationDays)
    {
        return new GetPromptResult
        {
            Messages =
            [
                new PromptMessage
                {
                    Role = Role.User,
                    Content = new TextContentBlock
                    {
                        Text = $"Using the Globomantics MCP tools, what are the best times to take a break in Hungary in {year} if I have {maxVacationDays} vacation days to spend?"
                    }
                }
            ]
        };
    }
}
