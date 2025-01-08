using System;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Repositories;

public interface IAppointmentDateRepository
{
    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId);
    public void AddAppointment(Appointment appointment);
    public List<Appointment> GetAppointmentsByPatientId(Guid PatientId);
    public Task<Pagination<Appointment>> GetAppointmentsByDoctorId(Guid DoctorId, int page);
    public Task<Pagination<Appointment>> GetAppointmentsByDoctorId(Guid doctorId, AppointmentStatus status, int page);
    public Appointment GetAppointmentsById(Guid appointmentId);
    public void UpdateStatusAppointment(Guid appointmentId, AppointmentStatus status);
    public List<Appointment> GetAllAppointments();
    public List<Appointment> GetAppointmentsByPatientId(Guid patientId, AppointmentStatus status);
    public Appointment GetAppointmentsByDoctorIdAndStartTime(Guid doctorId, DateTime StartTime); 
}
