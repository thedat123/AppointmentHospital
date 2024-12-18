using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface IDoctorService
{
    public Doctor getDoctorById(Guid doctorId);
    public List<Doctor> getAllDoctors();
    public List<TimeSlot> getTimeSlotByDoctorId(Guid doctorId);
    public String getDoctorNameByDoctorId(Guid doctorId);
}
