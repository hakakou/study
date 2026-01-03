using System;
using System.Collections.Generic;

namespace OData.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    // Non-nullable DateOnly and TimeOnly
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    // Nullable DateOnly and TimeOnly
    public DateOnly? RegistrationDeadline { get; set; }
    public TimeOnly? DoorOpenTime { get; set; }
    
    // Collections of DateOnly and TimeOnly
    public IList<DateOnly> AlternateDates { get; set; } = new List<DateOnly>();
    public IList<TimeOnly> SessionTimes { get; set; } = new List<TimeOnly>();
}
