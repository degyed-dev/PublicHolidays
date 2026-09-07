using PublicHolidays.Mcp.Server.Calendar;
using PublicHolidays.Mcp.Server.Holidays;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(settings: null);

// ── Holidays ──────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("nager", c =>
{
    c.BaseAddress = new Uri("https://nagerholidays.com");
    c.Timeout     = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<INagerHolidaysService, NagerHolidaysService>();

// ── MCP Server ────────────────────────────────────────────────────────────────
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithResources<CalendarResources>()
    .WithResources<HolidaysResource>()
    .WithTools<HolidayTools>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

Console.Error.WriteLine("MCP Server is running. Press ctrl+c to exit");

await app.RunAsync();
