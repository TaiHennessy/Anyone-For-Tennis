using AnyoneForTennis.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AnyoneForTennis.Models;

public partial class Coach
{
    public int CoachId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Biography { get; set; }

    public byte[]? Photo { get; set; }

    public static List<SelectListItem> GetCoaches(Hitdb1Context context)
    {
        /*return new List<SelectListItem>
        {
            new SelectListItem {Value = "CoachId", Text = "FirstName"}
        };*/

        var coaches = context.Coaches.
            Select(coach => new SelectListItem
            {
                Value = coach.CoachId.ToString(),
                Text = coach.FirstName
            }).ToList();
        return coaches;
    }
}
