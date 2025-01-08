using System;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface IAppointmentDateService
{
    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId);
    public void AddAppointment(Appointment appointment);
    public List<Appointment> GetAppointmentsByPatientId(Guid PatientId);
    public List<Appointment> GetAppointmentsByPatientId(Guid patientId, AppointmentStatus status);
    public Task<Pagination<Appointment>> GetAppointmentsByDoctorId(Guid DoctorId, int page);
    public Task<Pagination<Appointment>> GetAppointmentsByDoctorId(Guid doctorId, AppointmentStatus status, int page);
    public Appointment GetAppointmentsById(Guid appointmentId);
    public void UpdateStatusAppointment(Guid appointmentId, AppointmentStatus status);
    public List<Appointment> GetAllAppointments();
    public Appointment GetAppointmentsByDoctorIdAndStartTime(Guid doctorId, DateTime StartTime); 
    public DiagnosisHistory GetDiagnosisHistoriesByAppointmentID(Guid appointmentId);
    public List<Appointment> GetAppointmentsByDoctorIdAndDate(Guid doctorId, DateTime date);
}
