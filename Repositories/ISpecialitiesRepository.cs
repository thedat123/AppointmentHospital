using System;
using System.Collections.Generic;
using AppointmentHospital.Models;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Repositories
{
    public interface ISpecialitiesRepository
    {
        public Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page);
        public Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page, string searchTerm, string filter);
        public Specialities GetSpecialityById(int specialityId);
    }
}