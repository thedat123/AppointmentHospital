using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface IAppointmentDateService
{
    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId);
    public void AddAppointment(Appointment appointment);
    public List<Appointment> GetAppointmentsByPatientId(Guid PatientId);
    public List<Appointment> GetAppointmentsByDoctorId(Guid DoctorId);
}
