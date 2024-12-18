using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Repositories;

public interface IAppointmentDateRepository
{
    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId);
}
