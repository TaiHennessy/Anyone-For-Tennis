﻿using AnyoneForTennis.Data;
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

    public string? FullName => $"{FirstName} {LastName}";

    public ICollection<SchedulePlus> SchedulePlusPlus { get; set; } = new List<SchedulePlus>();
    public ICollection<Enrollment> Enrollments { get; set; } // Navigation Property

}
