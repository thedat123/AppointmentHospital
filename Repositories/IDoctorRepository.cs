using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Repositories;

public interface IDoctorRepository
{
    public List<Doctor> getAllDoctors();
    public Doctor getDoctorById(Guid doctorId);
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public string getDoctorNameByDoctorId(Guid doctorId);
    public List<string> GetDrugNameSearch(string search);
    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory);
}
