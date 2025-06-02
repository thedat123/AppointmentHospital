using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AppointmentHospital.Models;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Areas.Admin.Services
{
    public interface ISpecialityService
    {
        Task<Pagination<Specialities>> GetAllSpeciality(int page, string searchTerm);
        Task CreateSpecialityAsync(Specialities speciality);
        Task<bool> DeleteSpecialityAsync(int id);
        Task<Specialities> GetSpecialityAsync(int id);
        Task EditSpecialityAsync(int id, Specialities speciality);
    }
}