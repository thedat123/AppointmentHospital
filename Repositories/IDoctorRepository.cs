using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.ViewModels;

namespace AppointmentHospital.Repositories;

public interface IDoctorRepository
{
    public List<Doctor> getAllDoctors();
    public Doctor getDoctorById(Guid doctorId);
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public Task<Doctor> updateDoctor(Doctor request, string phoneNumber);
    public string getDoctorNameByDoctorId(Guid doctorId);
    public List<string> GetDrugNameSearch(string search);
    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory);
}
