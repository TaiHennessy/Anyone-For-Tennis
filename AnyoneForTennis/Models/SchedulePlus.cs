using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using AnyoneForTennis.Data;

namespace AnyoneForTennis.Models
{
    public class SchedulePlus
    {
        public int SchedulePlusId { get; set; } // Primary key
        public int ScheduleId { get; set; } // Foreign key from Schedule
        public DateTime DateTime { get; set; }
        public int Duration { get; set; } // Additional field example

        public int? CoachId { get; set; } // Foreign key from Coach

        // Navigation properties
        public Schedule Schedule { get; set; }

        [ForeignKey("CoachId")]
        public Coach Coach { get; set; }

       /* public static List<SelectListItem> GetCoaches(Hitdb1Context context)
        {
            *//*var coaches = context.Coaches.
                Select(coach => new SelectListItem
                {
                    Value = coach.CoachId.ToString(),
                    Text = coach.FullName
                }).ToList();
            return coaches;*//*

            return context.Coaches
                .Select(coach => new SelectListItem
                {
                    Value = coach.CoachId.ToString(),
                    Text = coach.FullName
                }).ToList();
        }*/
    }
}