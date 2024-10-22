using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AnyoneForTennis.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public string? Description { get; set; }

    // 1 to 1 relationship
    public SchedulePlus? SchedulePlus { get; set; }

    // Location Drop Down
    public static List<SelectListItem> GetLocations()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Value = "Court A", Text = "Court A" },
            new SelectListItem { Value = "Court B", Text = "Court B" },
            new SelectListItem { Value = "Court C", Text = "Court C" },
            new SelectListItem { Value = "Court D", Text = "Court D" }
        };
    }

    public ICollection<Enrollment> Enrollments { get; set; } // Navigation Property
}
