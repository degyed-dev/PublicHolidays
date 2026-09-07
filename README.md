# PublicHolidays MCP Server — .NET

A [Model Context Protocol](https://spec.modelcontextprotocol.io/) server that helps AI assistants plan holidays and long weekends for Hungary. Built with C# / .NET 10 and the official [ModelContextProtocol .NET SDK](https://github.com/modelcontextprotocol/csharp-sdk).

---

## Purpose

This server exposes Hungarian public holiday data to any MCP-compatible AI assistant (Claude Code, VS Code Copilot, etc.). It lets the assistant answer questions like:

- "When is the next public holiday in Hungary?"
- "What are the best long weekends I can take without using any vacation days?"
- "If I have 3 vacation days, what is the longest break I can create around Hungarian holidays in 2026?"

---

## Architecture

```
MCP Client (Claude Code / VS Code / …)
          |
          | MCP over stdio
          v
  PublicHolidays.Mcp.Server  (.NET 10)
          |
          v
  Nager.Date API  (https://nagerholidays.com)
```

The server communicates over **stdio** and is registered as a local MCP server in `~/.claude.json` or `.vscode/mcp.json`.

---

## Main features

- Fetch Hungarian public holidays from [Nager.Holidays](https://nagerholidays.com) with 24-hour in-memory caching
- Find long weekends (3+ consecutive days) that arise naturally from holidays — no vacation days needed
- Rank the best multi-day breaks for a given year and vacation-day budget
- Check whether any specific date is a public holiday
- Find the next upcoming public holiday from any given date
- Expose raw holiday data and calendar events as MCP Resources
- Offer a reusable vacation-planning prompt template

---

## MCP concepts used

### Tools

Four tools are registered. Each tool is callable by the AI assistant with strongly typed parameters.

| Tool | Parameters | Description |
|---|---|---|
| `find_long_weekends` | `year` (int, 2000–2100) | Returns all 3+ day stretches that require zero vacation days |
| `find_best_time_off` | `year` (int), `maxVacationDays` (int, 0–10) | Returns the best stretches combining weekends, holidays, and vacation days |
| `is_public_holiday` | `date` (yyyy-MM-dd) | Checks whether a specific date is a Hungarian public holiday |
| `get_next_public_holiday` | `fromDate` (yyyy-MM-dd) | Returns the next Hungarian public holiday on or after the given date |

### Prompts

One prompt template is registered. A prompt is a reusable message template the AI client can invoke with arguments to pre-fill a conversation.

| Prompt | Arguments | Rendered message |
|---|---|---|
| `best_time_off` | `year` (int), `maxVacationDays` (int) | "Using the PublicHolidays MCP tools, what are the best times to take a break in Hungary in {year} if I have {maxVacationDays} vacation days to spend?" |

### Resources

Three resources are exposed over the MCP resource protocol.

| URI | Description |
|---|---|
| `holidays://HU/{year}` | Hungarian public holidays for the given year, as JSON |
| `calendar://events` | All upcoming calendar events (static demo data) |
| `calendar://events/{id}` | A single calendar event by ID |

---

## Project structure

```
PublicHolidays/
  PublicHolidays.Mcp.Server/
    Program.cs                        ← Host setup and MCP server registration
    Holidays/
      HolidayTools.cs                 ← MCP tool registrations + calculation logic
      HolidayToolModels.cs            ← Tool result record types
      HolidaysModels.cs               ← Domain model (HolidayItem, HolidaysResponse)
      HolidaysResource.cs             ← holidays://HU/{year} resource
      HolidayPrompts.cs               ← best_time_off prompt
      NagerHolidaysService.cs         ← Nager.Date HTTP client + cache
      NagerHolidayDto.cs              ← Raw Nager.Date API DTO
    Calendar/
      CalendarResources.cs            ← calendar://events resources (static data)
    appsettings.json
    package.json                      ← npm dev script (MCP Inspector)

  PublicHolidays.Mcp.Server.Tests/
    Holidays/
      HolidayToolsTests.cs            ← 14 unit tests for tools and calculations
      NagerHolidaysServiceTests.cs    ← 5 unit tests for the HTTP service + cache
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- (Optional) [Node.js ≥ 18](https://nodejs.org) for the MCP Inspector dev script

---

## Installation

```bash
cd PublicHolidays
dotnet restore
```

---

## Local development

**With MCP Inspector** (recommended — starts a browser-based MCP testing UI):

```bash
npm run dev
```

This runs `npx @modelcontextprotocol/inspector dotnet run` inside the server project.

**Directly with .NET:**

```bash
dotnet run --project PublicHolidays.Mcp.Server/PublicHolidays.Mcp.Server.csproj
```

The server starts and waits for MCP messages on stdin/stdout.

---

## Build

```bash
dotnet build
```

---

## Running the MCP server

The server communicates over stdio. It is not a standalone HTTP server — it is launched by the MCP client.

**From Claude Code:** register it in `~/.claude.json` (see [Configuration](#configuration) below) and Claude Code will start it automatically.

**From VS Code:** the `.vscode/mcp.json` file in this repo registers the server for VS Code Copilot automatically.

---

## Testing

```bash
dotnet test PublicHolidays.Mcp.Server.Tests/PublicHolidays.Mcp.Server.Tests.csproj
```

The test suite has 19 tests covering:

- Long weekend calculation (Friday/Monday/Saturday/Sunday holidays, adjacent holidays)
- Best-time-off calculation (0, 1, 2 vacation days; mid-week only holidays; domination filter)
- Tool parameter validation (`maxVacationDays` out of range, invalid date format, invalid year)
- `is_public_holiday` (hit, miss, bad format)
- `get_next_public_holiday` (first holiday, mid-year, invalid date)
- `NagerHolidaysService` (success, HTTP 500, network error, 24 h cache)

---

## Example MCP usage

### find_long_weekends

```json
{ "year": 2026 }
```

```json
[
  {
    "holidayName": "Good Friday",
    "holidayDate": "2026-04-03",
    "startDate": "2026-04-03",
    "endDate": "2026-04-06",
    "consecutiveDays": 4,
    "vacationDaysRequired": 0
  },
  {
    "holidayName": "Labour day",
    "holidayDate": "2026-05-01",
    "startDate": "2026-05-01",
    "endDate": "2026-05-03",
    "consecutiveDays": 3,
    "vacationDaysRequired": 0
  }
]
```

Results are grouped per holiday — if multiple holidays fall in the same stretch, each appears as a separate entry.

### find_best_time_off

```json
{ "year": 2026, "maxVacationDays": 2 }
```

```json
[
  {
    "startDate": "2026-04-01",
    "endDate": "2026-04-06",
    "consecutiveDays": 6,
    "vacationDaysRequired": 2,
    "vacationDates": ["2026-04-01", "2026-04-02"],
    "publicHolidays": [
      "2026-04-03: Good Friday",
      "2026-04-05: Easter Sunday",
      "2026-04-06: Easter Monday"
    ]
  }
]
```

`vacationDates` lists the exact weekdays to take off to achieve the stretch.

### is_public_holiday

```json
{ "date": "2026-08-20" }
```

```json
{
  "date": "2026-08-20",
  "isPublicHoliday": true,
  "holidayName": "State Foundation Day",
  "dayOfWeek": "Thursday"
}
```

### get_next_public_holiday

```json
{ "fromDate": "2026-09-04" }
```

```json
{
  "holidayDate": "2026-10-23",
  "holidayName": "1956 Revolution Memorial Day",
  "dayOfWeek": "Friday",
  "daysUntil": 49
}
```

### holidays://HU/{year} resource

```
holidays://HU/2026
```

```json
{
  "countryCode": "HU",
  "year": 2026,
  "source": "Nager.Holidays",
  "holidays": [
    {
      "date": "2026-01-01",
      "name": "New Year's Day",
      "dayOfWeek": "Thursday",
      "nationalHoliday": true,
      "holidayTypes": ["Public"]
    }
  ]
}
```

---

## Example Claude Code usage

Ask Claude naturally — it selects the right tool automatically:

```
"Find all long weekends in Hungary in 2026 that don't require any vacation days."
"What are the best times to take a break in Hungary in 2026 if I have 3 vacation days?"
"Is 2026-08-20 a Hungarian public holiday?"
"When is the next Hungarian public holiday after today?"
```

**Combined with the calendar resource:**

```
"Show me the long weekends in 2026 where I don't have a Planning meeting."
"Find the best long weekend in 2026 where I need at most 2 vacation days and don't have Planning in my calendar."
```

Claude will call `find_long_weekends` or `find_best_time_off`, read `calendar://events`, and filter results against your calendar events.

---

## Configuration

### Claude Code (`~/.claude.json`)

```json
{
  "mcpServers": {
    "publicholidays": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/PublicHolidays/PublicHolidays.Mcp.Server/PublicHolidays.Mcp.Server.csproj",
        "--no-build"
      ]
    }
  }
}
```

Or from the Claude Code CLI:

```bash
claude mcp add \
  --transport stdio \
  publicholidays \
  dotnet -- run --project /path/to/PublicHolidays.Mcp.Server/PublicHolidays.Mcp.Server.csproj --no-build
```

### VS Code (`.vscode/mcp.json`)

The `.vscode/mcp.json` in the repo root already registers the server:

```json
{
  "servers": {
    "publicholidays-mcp-servers": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "PublicHolidays.Mcp.Server/PublicHolidays.Mcp.Server.csproj"]
    }
  }
}
```

### Using the `best_time_off` prompt

In Claude Code, type:

```
/mcp__publicholidays__best_time_off
```

You will be prompted for `year` and `maxVacationDays`. The server fills in the message template and sends it to the LLM.

You can also invoke it directly over MCP:

```json
{
  "method": "prompts/get",
  "params": {
    "name": "best_time_off",
    "arguments": { "year": "2026", "maxVacationDays": "3" }
  }
}
```

---

## Security considerations

- Holiday data is fetched from [Nager.Holidays](https://nagerholidays.com), an external public API. No API key is required.
- The server runs locally and communicates only over stdio — it is not exposed to the network.
- No secrets, credentials, or personally identifiable data are stored or transmitted.
- Calendar event data in this project is static demo data. In a production scenario, replace it with a proper calendar integration that respects data privacy requirements.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `Unable to reach the holidays service` | No internet connection or Nager.Holidays is down | Check network; retry later |
| `Year must be between 2000 and 2100` | Out-of-range year passed to a tool | Use a year within 2000–2100 |
| `Invalid date … Expected format: yyyy-MM-dd` | Wrong date format | Pass dates as `yyyy-MM-dd`, e.g. `2026-08-20` |
| Server does not appear in Claude Code | Path in `~/.claude.json` is wrong | Use an absolute path to the `.csproj` file |
| Build fails | Wrong .NET SDK version | Ensure .NET 10 SDK is installed |

---

## Smoke-test with Python

You can manually test any resource from the command line:

```python
import subprocess, json

proc = subprocess.Popen(
    ["dotnet", "run", "--project",
     "PublicHolidays.Mcp.Server/PublicHolidays.Mcp.Server.csproj", "--no-build"],
    stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL,
    text=True, bufsize=1
)

def send(msg):
    proc.stdin.write(json.dumps(msg) + "\n")
    proc.stdin.flush()

def recv():
    return json.loads(proc.stdout.readline())

send({"jsonrpc":"2.0","id":1,"method":"initialize",
      "params":{"protocolVersion":"2024-11-05","capabilities":{},
                "clientInfo":{"name":"test","version":"1.0"}}})
recv()
send({"jsonrpc":"2.0","method":"notifications/initialized","params":{}})

send({"jsonrpc":"2.0","id":2,"method":"resources/read",
      "params":{"uri":"holidays://HU/2026"}})
print(json.dumps(recv(), indent=2))

proc.stdin.close()
proc.wait()
```

---

## Future improvements

- Support additional countries beyond Hungary
- Configurable country code via environment variable or MCP argument
- Replace static calendar data with a real calendar integration (e.g., Google Calendar, Outlook)
- Add authentication for calendar resources
- Expose a `holidays://` resource listing all supported countries
