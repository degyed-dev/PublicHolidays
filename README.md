# PublicHolidays MCP Server

A [Model Context Protocol](https://spec.modelcontextprotocol.io/) server built with the official [ModelContextProtocol .NET SDK](https://github.com/modelcontextprotocol/csharp-sdk).

## Running the server

```bash
dotnet run --project PublicHolidays.Mcp.Server/PublicHolidays.Mcp.Server.csproj
```

The server communicates over **stdio** and is registered in `~/.claude.json` for Claude Code.

---

## Available Resources

| URI Template | Description |
|---|---|
| `calendar://events` | All upcoming PublicHolidays calendar events |
| `calendar://events/{id}` | A single calendar event by ID |
| `holidays://HU/{year}` | Hungarian public holidays for the given year (2000–2100) |

## Available Tools

| Tool | Description |
|---|---|
| `get_next_public_holiday` | Returns the next Hungarian public holiday on or after a given date |
| `is_public_holiday` | Checks whether a specific date is a Hungarian public holiday |
| `find_long_weekends` | Finds long weekends (3+ days) that arise naturally from holidays, no vacation days needed |
| `find_best_time_off` | Finds the best stretches of consecutive days off combining holidays, weekends, and a limited number of vacation days |

---

## Example: All Calendar Events

Request:
```
calendar://events
```

Response:
```json
[
  {
    "id": "1",
    "title": "Daily Standup",
    "start": "2026-09-04T09:00:00",
    "end": "2026-09-04T09:15:00",
    "location": "Conference Room A"
  },
  {
    "id": "3",
    "title": "Sprint Planning",
    "start": "2026-09-07T10:00:00",
    "end": "2026-09-07T12:00:00",
    "location": "Conference Room B"
  },
  {
    "id": "33",
    "title": "Q4 Planning Workshop",
    "start": "2026-09-28T09:00:00",
    "end": "2026-09-28T17:00:00",
    "location": "Offsite — Innovation Hub"
  }
]
```

---

## Example: Single Calendar Event by ID

Request:
```
calendar://events/7
```

Response:
```json
{
  "id": "7",
  "title": "Architecture Planning",
  "start": "2026-09-09T13:00:00",
  "end": "2026-09-09T14:30:00",
  "location": "Conference Room C"
}
```

When the ID does not exist:

Request:
```
calendar://events/99
```

Response:
```json
{
  "error": "Event '99' not found"
}
```

---

## Example: Hungarian Public Holidays

Request:
```
holidays://HU/2026
```

Response:
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
    },
    {
      "date": "2026-03-15",
      "name": "1848 Revolution Memorial Day",
      "dayOfWeek": "Sunday",
      "nationalHoliday": true,
      "holidayTypes": ["Public"]
    }
  ]
}
```

Holiday data is fetched from [Nager.Holidays](https://nagerholidays.com) and cached in memory for 24 hours.

---

## Example: Get Next Public Holiday

Request:
```json
{ "fromDate": "2026-09-04" }
```

Response:
```json
{
  "holidayDate": "2026-10-23",
  "holidayName": "1956 Revolution Memorial Day",
  "dayOfWeek": "Friday",
  "daysUntil": 49
}
```

---

## Example: Check if a Date is a Public Holiday

Request:
```json
{ "date": "2026-08-20" }
```

Response — it is a holiday:
```json
{
  "date": "2026-08-20",
  "isPublicHoliday": true,
  "holidayName": "State Foundation Day",
  "dayOfWeek": "Thursday"
}
```

Request:
```json
{ "date": "2026-09-04" }
```

Response — not a holiday:
```json
{
  "date": "2026-09-04",
  "isPublicHoliday": false,
  "holidayName": null,
  "dayOfWeek": "Friday"
}
```

---

## Example: Find Long Weekends

Request:
```json
{ "year": 2026 }
```

Response:
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
  },
  {
    "holidayName": "Christmas Day",
    "holidayDate": "2026-12-25",
    "startDate": "2026-12-25",
    "endDate": "2026-12-27",
    "consecutiveDays": 3,
    "vacationDaysRequired": 0
  }
]
```

Results are grouped per holiday — multiple holidays sharing the same stretch each appear as a separate entry.

---

## Example: Find Best Time Off

Request:
```json
{ "year": 2027, "maxVacationDays": 2 }
```

Response (top results, ranked by consecutive days then fewest vacation days):
```json
[
  {
    "startDate": "2027-03-24",
    "endDate": "2027-03-29",
    "consecutiveDays": 6,
    "vacationDaysRequired": 2,
    "vacationDates": ["2027-03-24", "2027-03-25"],
    "publicHolidays": [
      "2027-03-26: Good Friday",
      "2027-03-28: Easter Sunday",
      "2027-03-29: Easter Monday"
    ]
  },
  {
    "startDate": "2027-01-01",
    "endDate": "2027-01-05",
    "consecutiveDays": 5,
    "vacationDaysRequired": 2,
    "vacationDates": ["2027-01-04", "2027-01-05"],
    "publicHolidays": ["2027-01-01: New Year's Day"]
  }
]
```

`vacationDates` lists exactly which weekdays to take off to achieve the stretch. `maxVacationDays` accepts 0–10.

---

## Running the tests

```bash
dotnet test PublicHolidays.Mcp.Server.Tests/PublicHolidays.Mcp.Server.Tests.csproj
```

---

## Testing from an MCP client

You can smoke-test any resource with the Python script below:

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
