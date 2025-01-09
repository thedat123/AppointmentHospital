using System;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using AppointmentHospital.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentHospital.Repositories.Implement;

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _context;
    public DoctorRepository(AppDbContext context){
        _context = context; 
    }

    public async Task<List<Doctor>> getAllDoctors(string selectSpec){
        var query = _context.Doctors.AsQueryable();
        if(!selectSpec.IsNullOrEmpty()){
            int.TryParse(selectSpec, out int selectSpecInt);
            Specialization spec = (Specialization)Enum.ToObject(typeof(Specialization), selectSpecInt);
            query = query.Where(d => d.Specializaiton == spec);
        }
        return await query.ToListAsync();
    }

    public Doctor getDoctorById(Guid doctorId){
        return _context.Doctors
        .Include(d => d.User)
        .FirstOrDefault(d => d.DoctorId == doctorId)!;
    }
    public async Task<Doctor> updateDoctor(Doctor request, string phoneNumber) {
        var doctor = _context.Doctors.Where(d => d.DoctorId == request.DoctorId).FirstOrDefault();
        doctor.DateOfBirth = request.DateOfBirth;
        doctor.Description = request.Description;
        doctor.FullName = request.FullName;
        doctor.ExperienceYear = request.ExperienceYear;
        doctor.Gender = request.Gender;
        doctor.Degree = request.Degree;
        doctor.Specializaiton = request.Specializaiton;
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
