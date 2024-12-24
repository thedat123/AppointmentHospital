using System;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface IAppointmentDateService
{
    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId);
    public void AddAppointment(Appointment appointment);
    public List<Appointment> GetAppointmentsByPatientId(Guid PatientId);
    public List<Appointment> GetAppointmentsByDoctorId(Guid DoctorId);
    public List<Appointment> GetAppointmentsByDoctorId(Guid doctorId, AppointmentStatus status);
    public Appointment GetAppointmentsById(Guid appointmentId);
    public void UpdateStatusAppointment(Guid appointmentId, AppointmentStatus status);
    public List<Appointment> GetAllAppointments();
}
