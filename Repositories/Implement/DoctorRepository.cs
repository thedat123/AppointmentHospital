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

    public async Task<List<Doctor>> getAllDoctors(string selectSpec, int page)
    {
        var query = _context.Doctors.AsQueryable();
        int selectSpecInt = 0;

        if (!string.IsNullOrEmpty(selectSpec))
        {
            int.TryParse(selectSpec, out selectSpecInt);
            query = query.Where(d => d.SpecialityId == selectSpecInt);
        }

        return await Pagination<Doctor>.PaginatedList(query, page);
    }


    public Doctor getDoctorById(Guid doctorId){
        return _context.Doctors
        .Include(d => d.User)
        .FirstOrDefault(d => d.DoctorId == doctorId)!;
    }
    public async Task<Doctor> updateDoctor(Doctor request, string phoneNumber) {
        var doctor = _context.Doctors.Where(d => d.DoctorId == request.DoctorId).FirstOrDefault();
        doctor.FullName = request.FullName;
        doctor.Degree = request.Degree;
        doctor.SpecialityId = request.SpecialityId;
        doctor.User.PhoneNumber = phoneNumber;
        _context.Update(doctor);
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
