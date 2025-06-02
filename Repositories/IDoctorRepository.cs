using System;
using AppointmentHospital.Models;
using AppointmentHospital.ViewModels;

namespace AppointmentHospital.Repositories;

public interface IDoctorRepository
{
    public Task<List<Doctor>> getAllDoctors(string selectSpec, int page);
    public Doctor getDoctorById(Guid doctorId);
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public Task<Doctor> updateDoctor(Doctor request, string phoneNumber);
    public string getDoctorNameByDoctorId(Guid doctorId);
    public List<string> GetDrugNameSearch(string search);
    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory);
}
