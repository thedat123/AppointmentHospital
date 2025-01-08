using AppointmentHospital.Areas.Admin.DTOs.Statistic;

namespace AppointmentHospital.Areas.Admin.Repositories
{
    public interface IStatisticRepository
    {
        Task<Dictionary<string, int>> GetAmountAppointment(DateTime? singleDate, Dictionary<int, List<DateTime>> startAndLastDay, string customRange);
        Task<Dictionary<string, List<int>>> GetOldAndNewUser(DateTime? singleDate, Dictionary<int, List<DateTime>> startAndLastDay, string customRange);
        Task<Dictionary<string, int>> GetCompareAmountAppointment(List<DateTime> startAndEndDate, Dictionary<int, List<DateTime>> startAndEndDateOfEachWeekInMonth, int? month, int? year);
        Task<Dictionary<string, List<int>>> GetCompareOldAndNewUser(List<DateTime> startAndEndDate, Dictionary<int, List<DateTime>> startAndEndDateOfEachWeekInMonth, int? month, int? year);
        Task<Dictionary<string, List<TopDoctorStatistic>>> GetTopDoctorAppointment(DateTime? singleDate, Dictionary<int, List<DateTime>> startAndLastDay, string customRange);
    }
}
