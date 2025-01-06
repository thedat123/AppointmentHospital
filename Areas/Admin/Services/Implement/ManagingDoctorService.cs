using AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor;
using AppointmentHospital.Areas.Admin.Repositories;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Services.Implement
{
    public class ManagingDoctorService : IManagingDoctorService
    {
        private readonly IManagingDoctorRepository _managingDoctorRepository;
        public ManagingDoctorService(IManagingDoctorRepository managingDoctorRepository)
        {
            _managingDoctorRepository = managingDoctorRepository;
        }

        public async Task CreateNewDoctorAsync(ManagingDoctorRequest request)
        {
             await _managingDoctorRepository.CreateNewDoctorAsync(request);
        }
        
        public async Task DeleteDoctorAsync(Guid id)
        {
            await _managingDoctorRepository.DeleteDoctorAsync(id);
        }

        public async Task EditDoctorAsync(Guid id, ManagingDoctorRequest request)
        {
            await _managingDoctorRepository.EditDoctorAsync(id, request);
        }

        public async Task<Pagination<ManagingDoctorResponse> > GetAllDoctor(int page, string searchTerm, Specialization? specialization)
        {
            return  await _managingDoctorRepository.GetAllDoctor(page, searchTerm, specialization);
        }

        public async Task<ManagingDoctorResponse> GetDoctorAsync(Guid id)
        {
            return await _managingDoctorRepository.GetDoctorAsync(id);
        }

        public List<SelectListItem> GetSpecialization()
        {
            return _managingDoctorRepository.GetSpecialization();
        }
    }
}
