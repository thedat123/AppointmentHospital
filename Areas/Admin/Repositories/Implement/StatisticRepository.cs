using AppointmentHospital.Areas.Admin.DTOs.Statistic;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.WebSockets;
using System.Numerics;

namespace AppointmentHospital.Areas.Admin.Repositories.Implement
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly AppDbContext _appDbContext;
        public StatisticRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Dictionary<string, int>> GetAmountAppointment(DateTime? singleDate, Dictionary<int, List<DateTime>>? startAndLastDay, string customRange)
        {
            Dictionary<string, int> amountAppointmentByRangeDate = new Dictionary<string, int>();
            var query = _appDbContext.Appointments.AsQueryable();
            string keyName = "All Data";

            // Check if ANY filter is provided
            bool hasAnyFilter = singleDate.HasValue || 
                               (startAndLastDay != null && startAndLastDay.Keys.Count > 0) || 
                               !string.IsNullOrEmpty(customRange);

            if (singleDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime.Date == singleDate.Value.Date);
                keyName = singleDate.Value.ToString("MM/dd");
            }
            else if (startAndLastDay != null && startAndLastDay.Keys.Count > 0)
            {
                var keys = startAndLastDay.Keys;
                DateTime startDate = new DateTime();
                DateTime endDate = new DateTime();
                
                foreach (var key in keys)
                {
                    var listDate = startAndLastDay[key];
                    startDate = listDate[0];
                    endDate = listDate[1];
                }
                
                query = query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date);
                keyName = startAndLastDay.FirstOrDefault().Key.ToString();
            }
            else if (!string.IsNullOrEmpty(customRange))
            {
                try
                {
                    var dateRange = customRange.Split('|').Select(date => DateTime.Parse(date)).ToList();
                    if (dateRange.Count >= 2)
                    {
                        DateTime startDate = dateRange[0];
                        DateTime endDate = dateRange[1];
                        query = query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date);
                        keyName = customRange;
                    }
                }
                catch (Exception)
                {
                    // If parsing fails, use all data (no filter applied)
                    keyName = "All Data";
                }
            }
            
            // If no filters are applied, query will return all appointments with "All Data" key
            var amountAppointment = await query.CountAsync();
            amountAppointmentByRangeDate.Add(keyName, amountAppointment);
            return amountAppointmentByRangeDate;
        }

        public async Task<Dictionary<string, List<int>>> GetOldAndNewUser(DateTime? singleDate, Dictionary<int, List<DateTime>>? startAndLastDay, string customRange)
        {
            Dictionary<string, List<int>> amountOldAndNewUser = new Dictionary<string, List<int>>();

            var query = _appDbContext.Appointments.AsQueryable();
            var beforeParticularAppointmentDate = _appDbContext.Appointments.AsQueryable();
            int queryNewUser = 0;
            int queryOldUser = 0;
            string keyName = "All Data";

            if (singleDate.HasValue)
            {
                beforeParticularAppointmentDate = beforeParticularAppointmentDate.Where(a => a.AppointmentTime < singleDate.Value);
                queryNewUser = await query.Where(a => a.AppointmentTime.Date == singleDate.Value.Date && !beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                              .GroupBy(a => a.PatientId).CountAsync();
                queryOldUser = await query.Where(a => a.AppointmentTime.Date == singleDate.Value.Date && beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                              .GroupBy(a => a.PatientId).CountAsync();
                keyName = singleDate.Value.ToString("MM/dd");
            }
            else if (startAndLastDay != null && startAndLastDay.Keys.Count > 0)
            {
                var keys = startAndLastDay.Keys;
                DateTime startDate = new DateTime();
                DateTime endDate = new DateTime();
                
                foreach (var key in keys)
                {
                    var listDate = startAndLastDay[key];
                    startDate = listDate[0];
                    endDate = listDate[1];
                }
                
                beforeParticularAppointmentDate = query.Where(a => a.AppointmentTime.Date < startDate.Date);
                queryNewUser = await query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && !beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                        .GroupBy(a => a.PatientId).CountAsync();
                queryOldUser = await query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                            .GroupBy(a => a.PatientId).CountAsync();
                keyName = startAndLastDay.FirstOrDefault().Key.ToString();
            }
            else if (!string.IsNullOrEmpty(customRange))
            {
                try
                {
                    var dateRange = customRange.Split('|').Select(date => DateTime.Parse(date)).ToList();
                    if (dateRange.Count >= 2)
                    {
                        DateTime startDate = dateRange[0];
                        DateTime endDate = dateRange[1];
                        beforeParticularAppointmentDate = query.Where(a => a.AppointmentTime.Date < startDate.Date);
                        queryNewUser = await query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && !beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                                .GroupBy(a => a.PatientId).CountAsync();
                        queryOldUser = await query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                                    .GroupBy(a => a.PatientId).CountAsync();
                        keyName = customRange;
                    }
                    else
                    {
                        // Show all unique patients when no valid date range
                        queryNewUser = await query.GroupBy(a => a.PatientId).CountAsync();
                        queryOldUser = 0; // Can't distinguish old vs new for all data
                        keyName = "All Data";
                    }
                }
                catch (Exception)
                {
                    // Show all unique patients when parsing fails
                    queryNewUser = await query.GroupBy(a => a.PatientId).CountAsync();
                    queryOldUser = 0; // Can't distinguish old vs new for all data
                    keyName = "All Data";
                }
            }
            else
            {
                // No date filters provided - Show ALL unique patients
                var allPatients = await query.Select(a => a.PatientId).Distinct().ToListAsync();
                queryNewUser = allPatients.Count;
                queryOldUser = 0; // Can't distinguish old vs new for all data
                keyName = "All Data";
            }

            amountOldAndNewUser.Add(keyName, new List<int> { queryNewUser, queryOldUser });
            return amountOldAndNewUser;
        }

        public async Task<Dictionary<string, List<TopDoctorStatistic>>> GetTopDoctorAppointment(DateTime? singleDate, Dictionary<int, List<DateTime>>? startAndLastDay, string customRange)
        {
            var query = _appDbContext.Appointments.AsQueryable();
            string keyName = "All Data";

            if (singleDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime.Date == singleDate.Value && a.Status == AppointmentStatus.Completed);
                keyName = singleDate.Value.ToString("MM/dd");
            }
            else if (startAndLastDay != null && startAndLastDay.Keys.Count > 0)
            {
                DateTime startDay = new DateTime();
                DateTime endDay = new DateTime();
                
                foreach (var item in startAndLastDay)
                {
                    var itemValue = item.Value;
                    startDay = itemValue[0];
                    endDay = itemValue[1];
                }

                query = query.Where(a => a.AppointmentTime.Date >= startDay.Date && a.AppointmentTime.Date <= endDay.Date && a.Status == AppointmentStatus.Completed);
                keyName = startAndLastDay.FirstOrDefault().Key.ToString();
            }
            else if (!string.IsNullOrEmpty(customRange))
            {
                try
                {
                    var dateRange = customRange.Split('|').Select(date => DateTime.Parse(date)).ToList();
                    if (dateRange.Count >= 2)
                    {
                        DateTime startDay = dateRange[0];
                        DateTime endDay = dateRange[1];
                        query = query.Where(a => a.AppointmentTime.Date >= startDay.Date && a.AppointmentTime.Date <= endDay.Date && a.Status == AppointmentStatus.Completed);
                        keyName = customRange;
                    }
                    else
                    {
                        // Show all completed appointments when invalid range
                        query = query.Where(a => a.Status == AppointmentStatus.Completed);
                        keyName = "All Data";
                    }
                }
                catch (Exception)
                {
                    // Show all completed appointments when parsing fails
                    query = query.Where(a => a.Status == AppointmentStatus.Completed);
                    keyName = "All Data";
                }
            }
            else
            {
                // No date filters - Show ALL completed appointments
                query = query.Where(a => a.Status == AppointmentStatus.Completed);
                keyName = "All Data";
            }

            // Query for top doctor per specialization
            var doctorList = await query
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialities)
                .GroupBy(a => new
                {
                    SpecialityId = a.Doctor.SpecialityId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    SpecialityName = a.Doctor.Specialities.SpecialityName
                })
                .Select(g => new
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.DoctorName,
                    SpecialityId = g.Key.SpecialityId,
                    SpecialityName = g.Key.SpecialityName,
                    AmountAppointment = g.Count()
                })
                .ToListAsync();

            var topDoctorAmountEachSpecialization = doctorList
                .GroupBy(g => g.SpecialityId)
                .Select(g => g.OrderByDescending(d => d.AmountAppointment).FirstOrDefault())
                .Where(g => g != null)
                .Select(g => new TopDoctorStatistic 
                { 
                    AppointmentAmount = g.AmountAppointment, 
                    DoctorName = g.DoctorName, 
                    SpecialityName = g.SpecialityName 
                })
                .ToList();

            return new Dictionary<string, List<TopDoctorStatistic>>()
            {
                {keyName, topDoctorAmountEachSpecialization }
            };
        }

        public async Task<Dictionary<string, int>> GetCompareAmountAppointment(List<DateTime>? startAndEndDate, Dictionary<int, List<DateTime>>? startAndEndDateOfEachWeekInMonth, int? month, int? year)
        {
           try
            {
                Dictionary<string, int> compareAmountAppointment = new Dictionary<string, int>();

                if (startAndEndDate != null && startAndEndDate.Count == 2)
                {
                    // Use Aggregate to safely handle potential duplicates
                    var appointmentsByDate = await _appDbContext.Appointments
                        .Where(a => a.AppointmentTime.Date >= startAndEndDate[0].Date && a.AppointmentTime.Date <= startAndEndDate[1].Date)
                        .Select(a => a.AppointmentTime.Date)
                        .ToListAsync();

                    compareAmountAppointment = appointmentsByDate
                        .Aggregate(new Dictionary<string, int>(), (dict, date) =>
                        {
                            string key = date.ToString("MM/dd");
                            if (dict.ContainsKey(key))
                                dict[key]++;
                            else
                                dict[key] = 1;
                            return dict;
                        });
                }
                else if (month.HasValue && year.HasValue)
                {
                    // Initialize all months first
                    for (int i = 1; i <= 12; i++)
                    {
                        compareAmountAppointment[i.ToString()] = 0;
                    }

                    // Get actual data and update
                    var monthlyData = await _appDbContext.Appointments
                        .Where(a => a.AppointmentTime.Year == year)
                        .GroupBy(a => a.AppointmentTime.Month)
                        .Select(g => new { Month = g.Key, Count = g.Count() })
                        .ToListAsync();

                    foreach (var item in monthlyData)
                    {
                        compareAmountAppointment[item.Month.ToString()] = item.Count;
                    }
                }
                else if (startAndEndDateOfEachWeekInMonth != null && startAndEndDateOfEachWeekInMonth.Count != 0)
                {
                    foreach (var week in startAndEndDateOfEachWeekInMonth)
                    {
                        if (week.Value?.Count >= 2)
                        {
                            var startDate = week.Value[0];
                            var endDate = week.Value[1];
                            var appointmentAmount = await _appDbContext.Appointments
                                .Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date)
                                .CountAsync();

                            string weekKey = week.Key.ToString();
                            compareAmountAppointment[weekKey] = appointmentAmount; // Use indexer instead of Add
                        }
                    }
                }
                else
                {
                    var appointments = await _appDbContext.Appointments
                        .Select(a => a.AppointmentTime.Date)
                        .ToListAsync();

                    // Use Aggregate for safe dictionary building
                    compareAmountAppointment = appointments
                        .Aggregate(new Dictionary<string, int>(), (dict, date) =>
                        {
                            string key = date.ToString("MM/dd");
                            if (dict.ContainsKey(key))
                                dict[key]++;
                            else
                                dict[key] = 1;
                            return dict;
                        });
                }

                return compareAmountAppointment;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCompareAmountAppointment: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }
        
        public async Task<Dictionary<string, List<int>>> GetCompareOldAndNewUser(List<DateTime>? startAndEndDate, Dictionary<int, List<DateTime>>? startAndEndDateOfEachWeekInMonth, int? month, int? year)
        {
            Dictionary<string, List<int>> amountOldAndNewUser = new Dictionary<string, List<int>>();
            
            try
            {
                if (startAndEndDate != null && startAndEndDate.Count == 2)
                {
                    var startDate = startAndEndDate[0];
                    var endDate = startAndEndDate[1];
                    
                    while (startDate <= endDate)
                    {
                        string dateKey = startDate.Date.ToString("MM/dd");
                        
                        // Check if key already exists to avoid duplicates
                        if (!amountOldAndNewUser.ContainsKey(dateKey))
                        {
                            var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < startDate.Date);
                            var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date == startDate.Date && !userInPast.Any(ab => ab.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date == startDate.Date && userInPast.Any(ab => ab.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            amountOldAndNewUser.Add(dateKey, new List<int> { newUser, oldUser });
                        }
                        startDate = startDate.AddDays(1);
                    }
                }
                else if (month.HasValue && year.HasValue)
                {
                    // Show all 12 months for the specified year
                    for (int startMonth = 1; startMonth <= 12; startMonth++)
                    {
                        string monthKey = startMonth.ToString();
                        
                        var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < new DateTime(year.Value, startMonth, 1));
                        var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Month == startMonth && a.AppointmentTime.Year == year && !userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                        var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Month == startMonth && a.AppointmentTime.Year == year && userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                        amountOldAndNewUser[monthKey] = new List<int> { newUser, oldUser }; // Use indexer instead of Add
                    }
                }
                else if (startAndEndDateOfEachWeekInMonth != null && startAndEndDateOfEachWeekInMonth.Count != 0)
                {
                    foreach (var week in startAndEndDateOfEachWeekInMonth)
                    {
                        if (week.Value?.Count >= 2)
                        {
                            string weekKey = week.Key.ToString();
                            var weekValue = week.Value;
                            
                            var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < weekValue[0].Date);
                            var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= weekValue[0].Date && a.AppointmentTime.Date <= weekValue[1].Date && !userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= weekValue[0].Date && a.AppointmentTime.Date <= weekValue[1].Date && userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId).CountAsync();
                            
                            amountOldAndNewUser[weekKey] = new List<int> { newUser, oldUser }; // Use indexer instead of Add
                        }
                    }
                }
                else
                {
                    // Load all distinct dates first
                    var allDates = await _appDbContext.Appointments
                        .Select(a => a.AppointmentTime.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToListAsync();

                    // Process each unique date
                    foreach (var date in allDates)
                    {
                        string dateKey = date.ToString("MM/dd");
                        
                        // Use indexer to avoid duplicate key exception
                        if (!amountOldAndNewUser.ContainsKey(dateKey))
                        {
                            var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < date);
                            var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date == date && !userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date == date && userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            
                            amountOldAndNewUser[dateKey] = new List<int> { newUser, oldUser };
                        }
                        else
                        {
                            // If key exists, aggregate the values
                            var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < date);
                            var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date == date && !userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date == date && userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                    .GroupBy(a => a.PatientId)
                                                                    .CountAsync();
                            
                            // Add to existing values
                            amountOldAndNewUser[dateKey][0] += newUser;
                            amountOldAndNewUser[dateKey][1] += oldUser;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCompareOldAndNewUser: {ex.Message}");
                // Return empty dictionary on error
                return new Dictionary<string, List<int>>();
            }
            
            return amountOldAndNewUser;
        }
    }
}