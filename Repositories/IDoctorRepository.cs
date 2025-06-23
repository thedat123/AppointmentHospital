using System;
using AppointmentHospital.Models;
using AppointmentHospital.ViewModels;

namespace AppointmentHospital.Repositories;

public interface IDoctorRepository
{
    public Task<List<Doctor>> getAllDoctors(string selectSpec, string searchTerm, int page);
    public Doctor getDoctorById(Guid doctorId);
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public Task<Doctor> UpdateDoctor(DoctorInfoUpdate request, string phoneNumber, string imageUrl = null);
    public string getDoctorNameByDoctorId(Guid doctorId);
    public List<string> GetDrugNameSearch(string search);
    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory);
}
