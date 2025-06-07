using System;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using AppointmentHospital.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Repositories.Implement;

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _context;
    public DoctorRepository(AppDbContext context){
        _context = context; 
    }

    public async Task<List<Doctor>> getAllDoctors(string selectSpec, string searchTerm, int page)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(d => d.FullName.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(selectSpec))
        {
            int selectSpecInt;
            if (int.TryParse(selectSpec, out selectSpecInt))
            {
                query = query.Where(d => d.SpecialityId == selectSpecInt);
            }
        }

        return await Pagination<Doctor>.PaginatedList(query, page);
    }

    public Doctor getDoctorById(Guid doctorId){
        return _context.Doctors
        .Include(d => d.User)
        .FirstOrDefault(d => d.DoctorId == doctorId)!;
    }

    public async Task<Doctor> UpdateDoctor(DoctorInfoUpdate request, string phoneNumber)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User) // Đảm bảo có thể cập nhật số điện thoại
            .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId);

        if (doctor == null)
        {
            throw new Exception("Doctor not found");
        }

        // Cập nhật các trường từ DTO nếu có giá trị
        if (!string.IsNullOrEmpty(request.FullName))
            doctor.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Degree))
            doctor.Degree = request.Degree;

        if (request.SpecialityId.HasValue)
            doctor.SpecialityId = request.SpecialityId;

        if (!string.IsNullOrEmpty(request.ImagePath))
            doctor.ImagePath = request.ImagePath;

        if (!string.IsNullOrEmpty(request.Introduction))
            doctor.Introduction = request.Introduction;

        if (!string.IsNullOrEmpty(request.OrganizationMember))
            doctor.OrganizationMember = request.OrganizationMember;

        if (!string.IsNullOrEmpty(request.Expertise))
            doctor.Expertise = request.Expertise;

        if (!string.IsNullOrEmpty(request.Awards))
            doctor.Awards = request.Awards;

        if (!string.IsNullOrEmpty(request.ResearchProject))
            doctor.ResearchProject = request.ResearchProject;

        if (!string.IsNullOrEmpty(request.TrainingProcess))
            doctor.TrainingProcess = request.TrainingProcess;

        if (!string.IsNullOrEmpty(request.WorkExperience))
            doctor.WorkExperience = request.WorkExperience;

        if (!string.IsNullOrEmpty(phoneNumber))
            doctor.User.PhoneNumber = phoneNumber;

        await _context.SaveChangesAsync();

        return doctor;
    }


    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId){
        return _context.TimeSlots.Where(x => x.DoctorId == doctorId).ToList();
    }

    public string getDoctorNameByDoctorId(Guid doctorId){
        return _context.Doctors.Find(doctorId)?.FullName ?? string.Empty;
    }

    public List<string> GetDrugNameSearch(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return new List<string>();

        return _context.Drugs
            .Where(drug => EF.Functions.Like(drug.DrugName, $"%{search}%"))
            .Select(drug => drug.DrugName)
            .ToList() ?? new List<string>();
    }

    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory){
        _context.DiagnosisHistory.Add(diagnosisHistory);
        _context.SaveChanges();
    }

}
