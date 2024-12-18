using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface IAppointmentDateService
{
    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId);
}
