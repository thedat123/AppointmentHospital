using System;
using AppointmentHospital.Models;
using AppointmentHospital.ViewModels;

namespace AppointmentHospital.Services;

public interface IDoctorService
{
    public Doctor getDoctorById(Guid doctorId);
    public Task<List<Doctor>> getAllDoctors(string selectSpec, int page);
    public Task<Doctor> UpdateDoctor(DoctorInfoUpdate request, string phoneNumber);
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public string getDoctorNameByDoctorId(Guid doctorId);
    public List<string> GetDrugNameSearch(string search);
    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory);
}
