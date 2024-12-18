using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Repositories;

public interface IDoctorRepository
{
    public List<Doctor> getAllDoctors();
    public Doctor getDoctorById(Guid doctorId);
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public String getDoctorNameByDoctorId(Guid doctorId);
}
