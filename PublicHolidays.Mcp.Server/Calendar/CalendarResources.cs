using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace PublicHolidays.Mcp.Server.Calendar;

[McpServerResourceType]
public class CalendarResources
{
    [McpServerResource(UriTemplate = "calendar://events", Name = "calendar-events", Title = "Calendar Events", MimeType = "application/json")]
    public static TextResourceContents GetCalendarEvents() =>
        new()
        {
            Uri = "calendar://events",
            MimeType = "application/json",
            Text = """
            [
              { "id": "1",  "title": "Daily Standup",              "start": "2026-09-04T09:00:00", "end": "2026-09-04T09:15:00", "location": "Conference Room A" },
              { "id": "2",  "title": "Product Review",              "start": "2026-09-04T14:00:00", "end": "2026-09-04T15:00:00", "location": "Main Hall" },
              { "id": "3",  "title": "Sprint Planning",             "start": "2026-09-07T10:00:00", "end": "2026-09-07T12:00:00", "location": "Conference Room B" },
              { "id": "4",  "title": "Daily Standup",              "start": "2026-09-07T09:00:00", "end": "2026-09-07T09:15:00", "location": "Conference Room A" },
              { "id": "5",  "title": "1:1 with Manager",           "start": "2026-09-08T11:00:00", "end": "2026-09-08T11:30:00", "location": "Manager's Office" },
              { "id": "6",  "title": "Daily Standup",              "start": "2026-09-08T09:00:00", "end": "2026-09-08T09:15:00", "location": "Conference Room A" },
              { "id": "7",  "title": "Architecture Planning",      "start": "2026-09-09T13:00:00", "end": "2026-09-09T14:30:00", "location": "Conference Room C" },
              { "id": "8",  "title": "Daily Standup",              "start": "2026-09-09T09:00:00", "end": "2026-09-09T09:15:00", "location": "Conference Room A" },
              { "id": "9",  "title": "Team Lunch",                 "start": "2026-09-09T12:00:00", "end": "2026-09-09T13:00:00", "location": "Cafeteria" },
              { "id": "10", "title": "Code Review Session",        "start": "2026-09-10T15:00:00", "end": "2026-09-10T16:00:00", "location": "Conference Room A" },
              { "id": "11", "title": "Daily Standup",              "start": "2026-09-10T09:00:00", "end": "2026-09-10T09:15:00", "location": "Conference Room A" },
              { "id": "12", "title": "Sprint Review",              "start": "2026-09-11T14:00:00", "end": "2026-09-11T15:30:00", "location": "Main Hall" },
              { "id": "13", "title": "Sprint Retrospective",       "start": "2026-09-11T15:30:00", "end": "2026-09-11T16:30:00", "location": "Main Hall" },
              { "id": "14", "title": "Sprint Planning",            "start": "2026-09-14T10:00:00", "end": "2026-09-14T12:00:00", "location": "Conference Room B" },
              { "id": "15", "title": "Daily Standup",              "start": "2026-09-14T09:00:00", "end": "2026-09-14T09:15:00", "location": "Conference Room A" },
              { "id": "16", "title": "Hiring Interview",           "start": "2026-09-15T10:00:00", "end": "2026-09-15T11:00:00", "location": "HR Room" },
              { "id": "17", "title": "Daily Standup",              "start": "2026-09-15T09:00:00", "end": "2026-09-15T09:15:00", "location": "Conference Room A" },
              { "id": "18", "title": "Roadmap Planning",           "start": "2026-09-16T13:00:00", "end": "2026-09-16T15:00:00", "location": "Conference Room C" },
              { "id": "19", "title": "Daily Standup",              "start": "2026-09-16T09:00:00", "end": "2026-09-16T09:15:00", "location": "Conference Room A" },
              { "id": "20", "title": "UX Design Review",          "start": "2026-09-17T14:00:00", "end": "2026-09-17T15:00:00", "location": "Design Studio" },
              { "id": "21", "title": "Daily Standup",              "start": "2026-09-17T09:00:00", "end": "2026-09-17T09:15:00", "location": "Conference Room A" },
              { "id": "22", "title": "All-Hands Meeting",         "start": "2026-09-18T10:00:00", "end": "2026-09-18T11:30:00", "location": "Main Hall" },
              { "id": "23", "title": "Capacity Planning",         "start": "2026-09-21T13:00:00", "end": "2026-09-21T14:00:00", "location": "Conference Room B" },
              { "id": "24", "title": "Sprint Planning",           "start": "2026-09-21T10:00:00", "end": "2026-09-21T12:00:00", "location": "Conference Room B" },
              { "id": "25", "title": "Daily Standup",             "start": "2026-09-21T09:00:00", "end": "2026-09-21T09:15:00", "location": "Conference Room A" },
              { "id": "26", "title": "Security Review",           "start": "2026-09-22T14:00:00", "end": "2026-09-22T15:30:00", "location": "Conference Room A" },
              { "id": "27", "title": "Daily Standup",             "start": "2026-09-22T09:00:00", "end": "2026-09-22T09:15:00", "location": "Conference Room A" },
              { "id": "28", "title": "Release Planning",          "start": "2026-09-23T11:00:00", "end": "2026-09-23T12:30:00", "location": "Conference Room C" },
              { "id": "29", "title": "Daily Standup",             "start": "2026-09-23T09:00:00", "end": "2026-09-23T09:15:00", "location": "Conference Room A" },
              { "id": "30", "title": "Customer Demo",             "start": "2026-09-24T15:00:00", "end": "2026-09-24T16:00:00", "location": "Main Hall" },
              { "id": "31", "title": "Daily Standup",             "start": "2026-09-24T09:00:00", "end": "2026-09-24T09:15:00", "location": "Conference Room A" },
              { "id": "32", "title": "Sprint Review",             "start": "2026-09-25T14:00:00", "end": "2026-09-25T15:30:00", "location": "Main Hall" },
              { "id": "33", "title": "Q4 Planning Workshop",      "start": "2026-09-28T09:00:00", "end": "2026-09-28T17:00:00", "location": "Offsite — Innovation Hub" },
              { "id": "34", "title": "Sprint Planning",           "start": "2026-09-29T10:00:00", "end": "2026-09-29T12:00:00", "location": "Conference Room B" },
              { "id": "35", "title": "Budget Planning",           "start": "2026-09-30T13:00:00", "end": "2026-09-30T15:00:00", "location": "Finance Room" }
            ]
            """
        };

    [McpServerResource(UriTemplate = "calendar://events/{id}", Name = "calendar-event-by-id", Title = "Calendar Event by ID", MimeType = "application/json")]
    public static TextResourceContents GetCalendarEventById(string id) =>
        new()
        {
            Uri = $"calendar://events/{id}",
            MimeType = "application/json",
            Text = id switch
            {
                "1"  => """{"id":"1",  "title":"Daily Standup",             "start":"2026-09-04T09:00:00","end":"2026-09-04T09:15:00","location":"Conference Room A"}""",
                "2"  => """{"id":"2",  "title":"Product Review",             "start":"2026-09-04T14:00:00","end":"2026-09-04T15:00:00","location":"Main Hall"}""",
                "3"  => """{"id":"3",  "title":"Sprint Planning",            "start":"2026-09-07T10:00:00","end":"2026-09-07T12:00:00","location":"Conference Room B"}""",
                "4"  => """{"id":"4",  "title":"Daily Standup",             "start":"2026-09-07T09:00:00","end":"2026-09-07T09:15:00","location":"Conference Room A"}""",
                "5"  => """{"id":"5",  "title":"1:1 with Manager",          "start":"2026-09-08T11:00:00","end":"2026-09-08T11:30:00","location":"Manager's Office"}""",
                "6"  => """{"id":"6",  "title":"Daily Standup",             "start":"2026-09-08T09:00:00","end":"2026-09-08T09:15:00","location":"Conference Room A"}""",
                "7"  => """{"id":"7",  "title":"Architecture Planning",     "start":"2026-09-09T13:00:00","end":"2026-09-09T14:30:00","location":"Conference Room C"}""",
                "8"  => """{"id":"8",  "title":"Daily Standup",             "start":"2026-09-09T09:00:00","end":"2026-09-09T09:15:00","location":"Conference Room A"}""",
                "9"  => """{"id":"9",  "title":"Team Lunch",                "start":"2026-09-09T12:00:00","end":"2026-09-09T13:00:00","location":"Cafeteria"}""",
                "10" => """{"id":"10", "title":"Code Review Session",       "start":"2026-09-10T15:00:00","end":"2026-09-10T16:00:00","location":"Conference Room A"}""",
                "11" => """{"id":"11", "title":"Daily Standup",             "start":"2026-09-10T09:00:00","end":"2026-09-10T09:15:00","location":"Conference Room A"}""",
                "12" => """{"id":"12", "title":"Sprint Review",             "start":"2026-09-11T14:00:00","end":"2026-09-11T15:30:00","location":"Main Hall"}""",
                "13" => """{"id":"13", "title":"Sprint Retrospective",      "start":"2026-09-11T15:30:00","end":"2026-09-11T16:30:00","location":"Main Hall"}""",
                "14" => """{"id":"14", "title":"Sprint Planning",           "start":"2026-09-14T10:00:00","end":"2026-09-14T12:00:00","location":"Conference Room B"}""",
                "15" => """{"id":"15", "title":"Daily Standup",             "start":"2026-09-14T09:00:00","end":"2026-09-14T09:15:00","location":"Conference Room A"}""",
                "16" => """{"id":"16", "title":"Hiring Interview",          "start":"2026-09-15T10:00:00","end":"2026-09-15T11:00:00","location":"HR Room"}""",
                "17" => """{"id":"17", "title":"Daily Standup",             "start":"2026-09-15T09:00:00","end":"2026-09-15T09:15:00","location":"Conference Room A"}""",
                "18" => """{"id":"18", "title":"Roadmap Planning",          "start":"2026-09-16T13:00:00","end":"2026-09-16T15:00:00","location":"Conference Room C"}""",
                "19" => """{"id":"19", "title":"Daily Standup",             "start":"2026-09-16T09:00:00","end":"2026-09-16T09:15:00","location":"Conference Room A"}""",
                "20" => """{"id":"20", "title":"UX Design Review",         "start":"2026-09-17T14:00:00","end":"2026-09-17T15:00:00","location":"Design Studio"}""",
                "21" => """{"id":"21", "title":"Daily Standup",             "start":"2026-09-17T09:00:00","end":"2026-09-17T09:15:00","location":"Conference Room A"}""",
                "22" => """{"id":"22", "title":"All-Hands Meeting",        "start":"2026-09-18T10:00:00","end":"2026-09-18T11:30:00","location":"Main Hall"}""",
                "23" => """{"id":"23", "title":"Capacity Planning",        "start":"2026-09-21T13:00:00","end":"2026-09-21T14:00:00","location":"Conference Room B"}""",
                "24" => """{"id":"24", "title":"Sprint Planning",          "start":"2026-09-21T10:00:00","end":"2026-09-21T12:00:00","location":"Conference Room B"}""",
                "25" => """{"id":"25", "title":"Daily Standup",            "start":"2026-09-21T09:00:00","end":"2026-09-21T09:15:00","location":"Conference Room A"}""",
                "26" => """{"id":"26", "title":"Security Review",          "start":"2026-09-22T14:00:00","end":"2026-09-22T15:30:00","location":"Conference Room A"}""",
                "27" => """{"id":"27", "title":"Daily Standup",            "start":"2026-09-22T09:00:00","end":"2026-09-22T09:15:00","location":"Conference Room A"}""",
                "28" => """{"id":"28", "title":"Release Planning",         "start":"2026-09-23T11:00:00","end":"2026-09-23T12:30:00","location":"Conference Room C"}""",
                "29" => """{"id":"29", "title":"Daily Standup",            "start":"2026-09-23T09:00:00","end":"2026-09-23T09:15:00","location":"Conference Room A"}""",
                "30" => """{"id":"30", "title":"Customer Demo",            "start":"2026-09-24T15:00:00","end":"2026-09-24T16:00:00","location":"Main Hall"}""",
                "31" => """{"id":"31", "title":"Daily Standup",            "start":"2026-09-24T09:00:00","end":"2026-09-24T09:15:00","location":"Conference Room A"}""",
                "32" => """{"id":"32", "title":"Sprint Review",            "start":"2026-09-25T14:00:00","end":"2026-09-25T15:30:00","location":"Main Hall"}""",
                "33" => """{"id":"33", "title":"Q4 Planning Workshop",     "start":"2026-09-28T09:00:00","end":"2026-09-28T17:00:00","location":"Offsite — Innovation Hub"}""",
                "34" => """{"id":"34", "title":"Sprint Planning",          "start":"2026-09-29T10:00:00","end":"2026-09-29T12:00:00","location":"Conference Room B"}""",
                "35" => """{"id":"35", "title":"Budget Planning",          "start":"2026-09-30T13:00:00","end":"2026-09-30T15:00:00","location":"Finance Room"}""",
                _    => $$$"""{"error":"Event '{{{id}}}' not found"}"""
            }
        };
}
