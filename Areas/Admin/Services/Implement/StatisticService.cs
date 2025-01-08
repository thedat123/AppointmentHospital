using AppointmentHospital.Areas.Admin.DTOs.Statistic;
using AppointmentHospital.Areas.Admin.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace AppointmentHospital.Areas.Admin.Services.Implement
{
    public class StatisticService : IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;
        public StatisticService(IStatisticRepository statisticRepository)
        {
            _statisticRepository = statisticRepository;
        }
        public StatisticService() { }
        public List<SelectListItem> GetMonthsInYear()
        {
            List<SelectListItem> months = new List<SelectListItem>();   
            for (int i = 1; i <= 12; i++)
            {
                months.Add(new SelectListItem { Text = $"{new DateTime(2024,i,1).ToString("MMMM")}", Value = i.ToString() });
            }
            return months;
        }
        public List<SelectListItem> GetWeeksAllMonth(int year)
        {
            List<SelectListItem> weeks = new List<SelectListItem>();

            for (int i = 1; i <= 12; i++)
            {
                var firstDayofMonth = new DateTime(year, i, 1);
                var dayOfFirstDayInMonth = DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(firstDayofMonth);
                var lastDayofMonth = firstDayofMonth.AddMonths(1).AddDays(-1);
                List<DateTime> daysInMonth = new List<DateTime>();
                if ((int)dayOfFirstDayInMonth != 1) 
                {
                    int diff = (7-(dayOfFirstDayInMonth - DayOfWeek.Monday)) % 7;
                    var firstDayofFirstWeekInMonth = firstDayofMonth.AddDays(diff);
                    daysInMonth = Enumerable.Range(firstDayofFirstWeekInMonth.Date.Day - 1 , (lastDayofMonth.Day - firstDayofFirstWeekInMonth.Day + 1))
                                            .Select(index => firstDayofMonth.AddDays(index)).ToList(); 
                }
                else
                {
                    daysInMonth = Enumerable.Range(0, (lastDayofMonth.Day - firstDayofMonth.Day + 1))
                                            .Select(index => firstDayofMonth.AddDays(index)).ToList();
                }
                var dayOfLastDayOfWeekInMonth = DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(lastDayofMonth); 
                if ((int)dayOfLastDayOfWeekInMonth != 0) // Thứ cuối cùng của tháng không phải là chủ nhật
                {

                    var endDayofLastWeekInMonth = lastDayofMonth.AddDays(8 - (int)dayOfLastDayOfWeekInMonth); // 4/2/2024 
                    int different = (endDayofLastWeekInMonth - lastDayofMonth).Days;
                    var remindDayofLastWeekInNextMonth = Enumerable.Range(0, different)
                                                                   .Select(index => lastDayofMonth.AddDays(index)).ToList(); // tạo ra 1 list chứa những ngày của tháng sau vào tuần cuối của tháng trước
                    daysInMonth.AddRange(remindDayofLastWeekInNextMonth); // add những ngày của tháng sau (là ngày của tuần cuối cùng của tháng trước)
                }

                var dayOfEachWeek = daysInMonth.GroupBy(date => DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday))
                           .Select(group =>
                           {
                               var weekNumber = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(group.First(), CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday).ToString();
                               return new SelectListItem
                               {
                                   Value = weekNumber,
                                   Text = $"Week {weekNumber} from {group.First().ToString("MM/dd/yyyy")} to {group.Last().ToString("MM/dd/yyyy")}"
                               };
                           }
                           );
                weeks.AddRange(dayOfEachWeek.ToList());
            }
            return weeks;
        }


        public async Task<Dictionary<string, int>> GetAmountAppointment(DateTime? singleDate, int? month, int? week, int? year,string dateRange)
        {

            Dictionary<int, List<DateTime>> startAndLastDay = new Dictionary<int, List<DateTime>>();
            if (month != null)
            {
                startAndLastDay = GetStartAndLastDay(month, week, year);
            }
            else if (week != null)
            {
                startAndLastDay = GetStartAndLastDay(month, week, year);
            }
            var amountAppointment = await _statisticRepository.GetAmountAppointment(singleDate, startAndLastDay, dateRange);
            return amountAppointment;
        }

        public async Task<Dictionary<string, int>> GetCompareAmountAppointment(DateTime? singleDate, int? month, int? week, int? year)
        {
            List<DateTime> startAndEndDate = new List<DateTime>();
            Dictionary<int, List<DateTime>> startAndEndDateOfEachWeekInMonth = new Dictionary<int, List<DateTime>>();
            if (singleDate != null)
            {
                startAndEndDate = GetStartAndEndDateOfWeek(singleDate.Value);
            }
            else if (week != null)
            {
                startAndEndDateOfEachWeekInMonth = GetStartAndEndDateOfEachWeekInMonth(week.Value, year.Value);
            }
            return await _statisticRepository.GetCompareAmountAppointment(startAndEndDate, startAndEndDateOfEachWeekInMonth, month, year);
        }

        public List<DateTime> GetStartAndEndDateOfWeek(DateTime singleDate)
        {
            var diff = (7 + (singleDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startDate = singleDate.AddDays(-diff);
            var endDate = startDate.AddDays(6);
            return new List<DateTime>() { startDate, endDate };
        }
        public Dictionary<int, List<DateTime>> GetStartAndEndDateOfEachWeekInMonth(int? week, int? year)
        {
            //Handler riêng với week 1 là week của năm trước (có thể + month lên thêm 1)
            var startDate = ISOWeek.ToDateTime(year.Value, week.Value, DayOfWeek.Monday);
            int month;
            if(week == 1 && startDate.Month == 12)
            {
                month = 1;
            }
            else
            {
                month = startDate.Month;

            }
            var startDateInMonth = new DateTime(year.Value, month, 1);
            var endDateInMonth = startDateInMonth.AddMonths(1).AddDays(-1);
            if (startDateInMonth.DayOfWeek != DayOfWeek.Monday)
            {
                var diff = (7 - (startDateInMonth.DayOfWeek - DayOfWeek.Monday)) % 7;
                startDateInMonth = startDateInMonth.AddDays(diff);
            }
            if (endDateInMonth.DayOfWeek != DayOfWeek.Sunday)
            {
                var diff = (7 - (endDateInMonth.DayOfWeek - DayOfWeek.Monday)) % 7;
                endDateInMonth = endDateInMonth.AddDays(diff);
            }
            var currentDate = startDateInMonth;
            var dateDictionary = new Dictionary<int, List<DateTime>>();
            while (currentDate < endDateInMonth)
            {
                var weekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday); // tính tuần đầu tiên với 7 ngày đầy đủ
                if (!dateDictionary.ContainsKey(weekNum))
                {
                    dateDictionary.Add(weekNum, new List<DateTime> { currentDate, currentDate.AddDays(6) });
                }
                currentDate = currentDate.AddDays(1);
            }
            return dateDictionary;
        }

        public async Task<Dictionary<string, List<int>>> GetNewAndOldUser(DateTime? singleDate, int? month, int? week, int? year,string dateRange)
        {
            Dictionary<int, List<DateTime>> startAndLastDate = new Dictionary<int, List<DateTime>>();
            List<DateTime> customRange = new List<DateTime>();
            if (month != null)
            {
                startAndLastDate = GetStartAndLastDay(month, week, year);
            }
            else if (week != null)
            {
                startAndLastDate = GetStartAndLastDay(month, week, year);
            }
            var oldAndNewUser = await _statisticRepository.GetOldAndNewUser(singleDate, startAndLastDate, dateRange);
            return oldAndNewUser;
        }

        public async Task<Dictionary<string, List<int>>> GetCompareNewAndOldUser(DateTime? singleDate, int? month, int? week, int? year)
        {
            List<DateTime> startAndEndDateOfWeek = new List<DateTime>();
            Dictionary<int, List<DateTime>> startAndEndDateOfEachWeekInMonth = new Dictionary<int, List<DateTime>>();
            if (singleDate != null)
            {
                startAndEndDateOfWeek = GetStartAndEndDateOfWeek(singleDate.Value);
            }
            else if (week != null)
            {
                startAndEndDateOfEachWeekInMonth = GetStartAndEndDateOfEachWeekInMonth(week, year);
            }
            return await _statisticRepository.GetCompareOldAndNewUser(startAndEndDateOfWeek, startAndEndDateOfEachWeekInMonth, month, year);

        }

        public async Task<Dictionary<string, List<TopDoctorStatistic>>> GetTopDoctorAppointment(DateTime? singleDate, int? month, int? week, int? year, string dateRange)
        {
            Dictionary<int, List<DateTime>> startAndLastDay = new Dictionary<int, List<DateTime>>();
            List<DateTime> customRange = new List<DateTime>();
            if (week != null)
            {
                startAndLastDay = GetStartAndLastDay(month, week, year);
            }
            else if (month != null)
            {
                startAndLastDay = GetStartAndLastDay(month, week, year);
            }
            var response = await _statisticRepository.GetTopDoctorAppointment(singleDate, startAndLastDay, dateRange);
            return response;
        }

        private Dictionary<int, List<DateTime>> GetStartAndLastDay(int? month, int? week, int? year)
        {
            Dictionary<int, List<DateTime>> startAndLastDay = new Dictionary<int, List<DateTime>>();
            List<DateTime> dateTimes = new List<DateTime>();
            if (month != null)
            {
                var startDayInMonth = new DateTime(year ?? 2024, month ?? 1, 1);
                var endDayInMonth = startDayInMonth.AddMonths(1).AddDays(-1);
                dateTimes.Add(startDayInMonth);
                dateTimes.Add(endDayInMonth);
                startAndLastDay.Add(month ?? 1, dateTimes);
            }
            else
            {
                List<DateTime> daysOfParticularWeek = new List<DateTime>();
                var firstDayOfYear = new DateTime(year ?? 2024, 1, 1);
                DateTime fisrtDayOfFirstWeek;
                if ((int)firstDayOfYear.DayOfWeek == 1)
                {
                    fisrtDayOfFirstWeek = firstDayOfYear;
                }
                else
                {
                    var passDayOfPreviousYear = 8 - (int)firstDayOfYear.DayOfWeek;
                    fisrtDayOfFirstWeek = firstDayOfYear.AddDays(passDayOfPreviousYear);
                }
                var firstDayOfParticularWeek = fisrtDayOfFirstWeek.AddDays(((double)week - 1) * 7);

                for (int i = 0; i < 7; i++)
                {
                    daysOfParticularWeek.Add(firstDayOfParticularWeek.AddDays(i));
                }
                dateTimes.Add(daysOfParticularWeek.First());
                dateTimes.Add(daysOfParticularWeek.Last());
                startAndLastDay.Add(week ?? 1, dateTimes);
            }
            return startAndLastDay;

        }
    }
}

