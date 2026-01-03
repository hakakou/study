using System;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using OData.Models;

namespace OData.Controllers;

public class EventsController : ODataController
{
    private static readonly IQueryable<Event> _events;

    static EventsController()
    {
        var list = new List<Event>();
        for (int i = 0; i < 100; i++)
        {
            var e = new Event
            {
                Id = i,
                Title = ".NET Conference 2025",
                Location = "Seattle Convention Center",
                EventDate = new DateOnly(2025, 11, 15).AddDays(i),
                StartTime = new TimeOnly(9, 0, 0),
                EndTime = new TimeOnly(17, 30, 0),
                RegistrationDeadline = new DateOnly(2025, 11, 1).AddDays(i),
                DoorOpenTime = new TimeOnly(8, 30, 0),
                AlternateDates = new List<DateOnly>
                    {
                        new DateOnly(2025, 11, 16).AddDays(i),
                        new DateOnly(2025, 11, 17).AddDays(i)
                    },
                SessionTimes = new List<TimeOnly>
                    {
                        new TimeOnly(10, 0, 0),
                        new TimeOnly(14, 0, 0),
                        new TimeOnly(16, 0, 0)
                    }
            };
            list.Add(e);
        }

        _events = list.AsQueryable();
    }

    // GET: odata/Events
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_events);
    }

    // GET: odata/Events(1)
    [EnableQuery]
    public IActionResult Get(int key)
    {
        var evt = _events.FirstOrDefault(e => e.Id == key);
        return evt == null ? NotFound() : Ok(evt);
    }

    // POST: odata/Events
    [HttpPost]
    public IActionResult Post([FromBody] Event evt)
    {
        evt.Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1;
        //_events.Add(evt);
        return Created(evt);
    }

    // Function: GetEventsInDateRange
    [EnableQuery]
    //[HttpGet("odata/Events/GetEventsInDateRange(startDate={startDate},endDate={endDate},preferredTime={preferredTime})")]
    [HttpGet]
    public IActionResult GetEventsInDateRange([FromRoute] DateOnly startDate, [FromRoute] DateOnly endDate, [FromRoute] TimeOnly? preferredTime)
    {
        var filtered = _events.Where(e =>
            e.EventDate >= startDate &&
            e.EventDate <= endDate);

        if (preferredTime.HasValue)
        {
            filtered = filtered.Where(e =>
                e.StartTime <= preferredTime.Value && e.EndTime >= preferredTime.Value);
        }

        return Ok(filtered);
    }
}
