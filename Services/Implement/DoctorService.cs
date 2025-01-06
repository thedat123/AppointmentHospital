using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository doctorRepository;

    public DoctorService(IDoctorRepository _doctorRepository){
        doctorRepository = _doctorRepository;
    }

    public Doctor getDoctorById(Guid doctorId)
    {
        return doctorRepository.getDoctorById(doctorId);
    }

    public List<Doctor> getAllDoctors(){
        return doctorRepository.getAllDoctors();
    }

    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId){
        return doctorRepository.getTimeSlotByDoctorId(doctorId);
    }

    public string getDoctorNameByDoctorId(Guid doctorId){
        return doctorRepository.getDoctorNameByDoctorId(doctorId);
    }

    public List<string> GetDrugNameSearch(string search){
        return doctorRepository.GetDrugNameSearch(search);
    }

    public void AddDiagnosticHistory(DiagnosisHistory diagnosisHistory){
        doctorRepository.AddDiagnosticHistory(diagnosisHistory);
    }
}
