using AppointmentHospital.Areas.Admin.DTOs.Statistic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Services
{
    public interface IStatisticService
    {
        List<SelectListItem> GetMonthsInYear();
        List<SelectListItem> GetWeeksAllMonth(int year);
        Task<Dictionary<string, int>> GetAmountAppointment(DateTime? singleDate, int? month, int? week, int? year);
        Task<Dictionary<string, int>> GetCompareAmountAppointment(DateTime? singleDate, int? month, int? week, int? year);
        Task<Dictionary<string, List<int>>> GetNewAndOldUser(DateTime? singleDate, int? month, int? week, int? year);
        Task<Dictionary<string, List<int>>> GetCompareNewAndOldUser(DateTime? singleDate, int? month, int? week, int? year);
        Task<Dictionary<string, List<TopDoctorStatistic>>> GetTopDoctorAppointment(DateTime? singleDate, int? month, int? week, int? year);


    }
}
