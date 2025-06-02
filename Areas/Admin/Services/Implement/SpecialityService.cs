using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AppointmentHospital.Areas.Admin.Repositories;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppointmentHospital.Models;

namespace AppointmentHospital.Areas.Admin.Services.Implement
{
    public class SpecialityService : ISpecialityService
    {
        private readonly ISpecialityRepository _specialityRepository;

        public SpecialityService(ISpecialityRepository specialityRepository)
        {
            _specialityRepository = specialityRepository;
        }

        public async Task<Pagination<Specialities>> GetAllSpeciality(int page, string searchTerm)
        {
            return await _specialityRepository.GetAllSpeciality(page, searchTerm);
        }

        public async Task CreateSpecialityAsync(Specialities speciality)
        {
            await _specialityRepository.CreateSpecialityAsync(speciality);
        }

        public async Task<bool> DeleteSpecialityAsync(int id)
        {
            return await _specialityRepository.DeleteSpecialityAsync(id);
        }

        public async Task<Specialities> GetSpecialityAsync(int id)
        {
            return await _specialityRepository.GetSpecialityAsync(id);
        }

        public async Task EditSpecialityAsync(int id, Specialities speciality)
        {
            await _specialityRepository.EditSpecialityAsync(id, speciality);
        }
        
    }
}