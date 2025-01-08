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
            var keys = startAndLastDay.Keys;
            var query = _appDbContext.Appointments.AsQueryable();
            if (singleDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime.Date == singleDate.Value.Date);
            }
            else
            {
                 DateTime startDate = new DateTime();
                 DateTime endDate = new DateTime();
                if (startAndLastDay.Keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        var listDate = startAndLastDay[key];
                        startDate = listDate[0];
                        endDate = listDate[1];
                    }
                }
                else
                {
                    var dateRange = customRange.Split('|').Select(date => DateTime.Parse(date)).ToList();
                    startDate = dateRange[0];
                    endDate = dateRange[1];
                }
                query = query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date);
            }
            var amountAppointment = await query.CountAsync();
            amountAppointmentByRangeDate.Add(singleDate.HasValue ? singleDate.Value.ToString("MM/dd") : startAndLastDay.Keys.Count > 0 ? startAndLastDay.FirstOrDefault().Key.ToString() : customRange, amountAppointment);
            return amountAppointmentByRangeDate;
        }

        public async Task<Dictionary<string, List<int>>> GetOldAndNewUser(DateTime? singleDate, Dictionary<int, List<DateTime>>? startAndLastDay, string customRange)
        {
            Dictionary<string, List<int>> amountOldAndNewUser = new Dictionary<string, List<int>>();

            var query = _appDbContext.Appointments.AsQueryable();
            var beforeParticularAppointmentDate = _appDbContext.Appointments.AsQueryable();
            var keys = startAndLastDay.Keys;
            int queryNewUser = 0;
            int queryOldUser = 0;

            if (singleDate.HasValue)
            {
                beforeParticularAppointmentDate = beforeParticularAppointmentDate.Where(a => a.AppointmentTime < singleDate.Value);
                queryNewUser = await query.Where(a => a.AppointmentTime.Date == singleDate.Value.Date && !beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                              .GroupBy(a => a.PatientId).CountAsync();
                queryOldUser = await query.Where(a => a.AppointmentTime.Date == singleDate.Value.Date && beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                              .GroupBy(a => a.PatientId).CountAsync();
                amountOldAndNewUser.Add(singleDate.Value.ToString("MM/dd"), new List<int> { queryNewUser, queryOldUser });
            }
            else
            {
                DateTime startDate = new DateTime();
                DateTime endDate = new DateTime();
                if (startAndLastDay.Keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        var listDate = startAndLastDay[key];
                        startDate = listDate[0];
                        endDate = listDate[1];
                    }
                }
                else
                {
                    var dateRange = customRange.Split('|').Select(date => DateTime.Parse(date)).ToList();
                    startDate = dateRange[0];
                    endDate = dateRange[1];
                }
                beforeParticularAppointmentDate = query.Where(a => a.AppointmentTime.Date < startDate.Date);
                queryNewUser = await query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && !beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                        .GroupBy(a => a.PatientId).CountAsync();
                queryOldUser = await query.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && beforeParticularAppointmentDate.Any(ab => ab.PatientId == a.PatientId))
                                            .GroupBy(a => a.PatientId).CountAsync();
                amountOldAndNewUser.Add(startAndLastDay.Keys.Count > 0 ? startAndLastDay.FirstOrDefault().Key.ToString() : customRange, new List<int> { queryNewUser, queryOldUser });
            }
            return amountOldAndNewUser;
        }
        public async Task<Dictionary<string, List<TopDoctorStatistic>>> GetTopDoctorAppointment(DateTime? singleDate, Dictionary<int, List<DateTime>> startAndLastDay, string customRange)
        {
            var query = _appDbContext.Appointments.AsQueryable();

            if (singleDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime.Date == singleDate.Value && a.Status == AppointmentStatus.Completed);
            }
            else
            {
                DateTime startDay = new DateTime();
                DateTime endDay = new DateTime();
                if (startAndLastDay.Keys.Count > 0)
                {
                    foreach (var item in startAndLastDay)
                    {
                        var itemValue = item.Value;
                        startDay = itemValue[0];
                        endDay = itemValue[1];
                    }
                }
                else
                {
                    var dateRange = customRange.Split('|').Select(date => DateTime.Parse(date)).ToList();
                    startDay = dateRange[0];
                    endDay = dateRange[1];
                }

                query = query.Where(a => a.AppointmentTime.Date >= startDay.Date && a.AppointmentTime.Date <= endDay.Date && a.Status == AppointmentStatus.Completed);
            }
            // Step 1: Query for top doctor per specialization
            var doctorList = await query.GroupBy(a => new
            {
                Specialization = a.Doctor.Specializaiton,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.FullName
            }).Select(g => new
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.DoctorName,
                Specialization = g.Key.Specialization,
                AmountAppointment = g.Count()
            }).ToListAsync();

            var topDoctorAmountEachSpecialization = doctorList.GroupBy(g => g.Specialization)
            .Select(g => g.OrderByDescending(g => g.AmountAppointment).FirstOrDefault())
            .Select(g => new TopDoctorStatistic { AppointmentAmount = g.AmountAppointment, DoctorName = g.DoctorName, Specialization = g.Specialization.ToString() }).ToList();

            var key = singleDate.HasValue ? singleDate.Value.ToString("MM/dd") : startAndLastDay.Keys.Count > 0 ? startAndLastDay.FirstOrDefault().Key.ToString() : customRange;

            // Tạo Dictionary
            return new Dictionary<string, List<TopDoctorStatistic>>()
            {
                {key, topDoctorAmountEachSpecialization }
            };
        }

        public async Task<Dictionary<string, int>> GetCompareAmountAppointment(List<DateTime> startAndEndDate, Dictionary<int, List<DateTime>> startAndEndDateOfEachWeekInMonth, int? month, int? year)
        {
            Dictionary<string, int> compareAmountAppointment = new Dictionary<string, int>();
            if (startAndEndDate != null && startAndEndDate.Count == 2)
            {
                compareAmountAppointment = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= startAndEndDate[0].Date && a.AppointmentTime.Date <= startAndEndDate[1].Date)
                                                                           .GroupBy(a => a.AppointmentTime.Date)
                                                                           .ToDictionaryAsync(g => g.Key.Date.ToString("MM/dd"), g => g.Count());
            }
            if (month.HasValue)
            {
                compareAmountAppointment = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Year == year)
                                          .GroupBy(a => a.AppointmentTime.Month)
                                          .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
            }
            if (startAndEndDateOfEachWeekInMonth.Count != 0)
            {
                foreach (var week in startAndEndDateOfEachWeekInMonth)
                {
                    var startDate = week.Value[0];
                    var endDate = week.Value[1];
                    var appointmentAmount = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date).CountAsync();
                    compareAmountAppointment.Add(week.Key.ToString(), appointmentAmount);
                }
            }
            return compareAmountAppointment;
        }
        public async Task<Dictionary<string, List<int>>> GetCompareOldAndNewUser(List<DateTime> startAndEndDate, Dictionary<int, List<DateTime>> startAndEndDateOfEachWeekInMonth, int? month, int? year)
        {
            Dictionary<string, List<int>> amountOldAndNewUser = new Dictionary<string, List<int>>();
            // tìm từng ngày một 
            if (startAndEndDate != null && startAndEndDate.Count == 2)
            {
                var startDate = startAndEndDate[0];
                var endDate = startAndEndDate[1];
                while (startDate <= endDate)
                {
                    var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < startDate.Date);
                    var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= startDate.Date && a.AppointmentTime.Date <= endDate.Date && !userInPast.Any(ab => ab.PatientId == a.PatientId))
                                                            .GroupBy(a => new { Date = a.AppointmentTime.Date, PatientId = a.PatientId })
                                                            .CountAsync();
                    var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= startDate && a.AppointmentTime.Date <= endDate && userInPast.Any(ab => ab.PatientId == a.PatientId))
                                                            .GroupBy(a => new { Date = a.AppointmentTime.Date, PatientId = a.PatientId })
                                                            .CountAsync();
                    amountOldAndNewUser.Add(startDate.Date.ToString("MM/dd"), new List<int> { newUser, oldUser });
                    startDate = startDate.AddDays(1);
                }
            }
            if (month.HasValue)
            {
                var startMonth = 1;
                while (startMonth <= 12)
                {
                    var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < new DateTime(year.Value, startMonth, 1));
                    var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Month == startMonth && a.AppointmentTime.Year == year && !userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                  .GroupBy(a => a.PatientId)
                                                                  .CountAsync();
                    var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Month == startMonth && a.AppointmentTime.Year == year && userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                                  .GroupBy(a => a.PatientId)
                                                                  .CountAsync();
                    amountOldAndNewUser.Add(startMonth.ToString(), new List<int> { newUser, oldUser });
                    startMonth++;
                }
            }
            if (startAndEndDateOfEachWeekInMonth.Count != 0)
            {
                foreach (var week in startAndEndDateOfEachWeekInMonth)
                {
                    var weekValue = week.Value;
                    var userInPast = _appDbContext.Appointments.Where(a => a.AppointmentTime.Date < weekValue[0].Date);
                    var newUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= weekValue[0].Date && a.AppointmentTime.Date <= weekValue[1].Date && !userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                            .GroupBy(a => a.PatientId)
                                                            .CountAsync();
                    var oldUser = await _appDbContext.Appointments.Where(a => a.AppointmentTime.Date >= weekValue[0].Date && a.AppointmentTime.Date <= weekValue[1].Date && userInPast.Any(uip => uip.PatientId == a.PatientId))
                                                            .GroupBy(a => a.PatientId).CountAsync();
                    amountOldAndNewUser.Add(week.Key.ToString(), new List<int> { newUser, oldUser });
                }
            }
            return amountOldAndNewUser;
        }

    }
}
