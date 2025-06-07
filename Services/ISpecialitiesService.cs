using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Models;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Services
{
    public interface ISpecialitiesService
    {
        public Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page);
        public Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page, string searchTerm, string filter);
        public Specialities GetSpecialityById(int specialityId);
    }
}