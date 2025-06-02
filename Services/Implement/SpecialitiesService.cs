using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Helpers;
using AppointmentHospital.Repositories;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services.Implement
{
    public class SpecialitiesService : ISpecialitiesService
    {
        private readonly ISpecialitiesRepository specialitiesRepository;
        public SpecialitiesService(ISpecialitiesRepository specialitiesRepository){
            this.specialitiesRepository = specialitiesRepository;
        }
        public async Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page){
            return await specialitiesRepository.GetAllSpecialitiesAsync(page);
        }

        public Specialities GetSpecialityById(int specialityId){
            return specialitiesRepository.GetSpecialityById(specialityId);
        }
    }
}