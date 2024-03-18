using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain {
    

    /*"startHour": "9:00:00",
        "endHour": "18:00:00",
        "minGap": 15,
        "maxHoursPerDay": 5,
        "maxHoursPerTypePerDay": {"A": 3, "B": 2},
        "minTimeFrame": 15*/
    public class ScheduleSetting {
        private const int DEAFAULT_TIME_SLOT_LEN = 15;
        private const int DEAFAULT_MAX_HOURS = 5;
        private const int NUM_OF_DAYS_EXEPT_SUNDAY = 6;
        private const int DEAFAULT_STARTING_MIN_SECOND = 0;
        private const int DEAFAULT_STARTING_HOUR = 9;
        private const int DEAFAULT_ENDING_HOUR = 18;
        private const int LAST_INDEX = -1;
        [Key] public int Id { get; set; }
        public DateTime StartHour { get; set; }
        public DateTime EndHour { get; set; }
        public int MinGap { get; set; } = DEAFAULT_TIME_SLOT_LEN;
        public int MaxHoursPerDay { get; set; } = DEAFAULT_MAX_HOURS;
        public int MinTimeFrame { get; set; } = DEAFAULT_TIME_SLOT_LEN;

        public ScheduleSetting()
        {
            // Calculate the last Sunday
            DateTime currentDate = DateTime.Now;
            DateTime lastSunday = currentDate.AddDays(-(int)currentDate.DayOfWeek);
            while (lastSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                lastSunday = lastSunday.AddDays(LAST_INDEX);
            }

            // Set the StartHour to last Sunday at 9:00:00
            StartHour = new DateTime(lastSunday.Year, lastSunday.Month, lastSunday.Day, DEAFAULT_STARTING_HOUR, DEAFAULT_STARTING_MIN_SECOND, DEAFAULT_STARTING_MIN_SECOND);

            // Calculate the upcoming Saturday
            DateTime upcomingSaturday = lastSunday.AddDays(NUM_OF_DAYS_EXEPT_SUNDAY);

            // Set the EndHour to the upcoming Saturday at 18:00:00
            EndHour = new DateTime(upcomingSaturday.Year, upcomingSaturday.Month, upcomingSaturday.Day, DEAFAULT_ENDING_HOUR, DEAFAULT_STARTING_MIN_SECOND, DEAFAULT_STARTING_MIN_SECOND);
        }
    }
    

}
