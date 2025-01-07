using System;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement;

public class AppointmentDateService : IAppointmentDateService
{
    private readonly IAppointmentDateRepository appointmentRepository;

    public AppointmentDateService(IAppointmentDateRepository _appointmentRepository){
        appointmentRepository = _appointmentRepository;
    }

    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId){
        return appointmentRepository.GetAppointmentByDoctorId(DoctorId);
    }

    public void AddAppointment(Appointment appointment){
        appointmentRepository.AddAppointment(appointment);
    }

    public List<Appointment> GetAppointmentsByPatientId(Guid PatientId){
        return appointmentRepository.GetAppointmentsByPatientId(PatientId);
    }

    public List<Appointment> GetAppointmentsByPatientId(Guid PatientId, AppointmentStatus status){
        return appointmentRepository.GetAppointmentsByPatientId(PatientId, status);
    }

    public List<Appointment> GetAppointmentsByDoctorId(Guid DoctorId){
        return appointmentRepository.GetAppointmentsByDoctorId(DoctorId);
    }

    public List<Appointment> GetAppointmentsByDoctorId(Guid doctorId, AppointmentStatus status){
        return appointmentRepository.GetAppointmentsByDoctorId(doctorId, status);
    }

    public Appointment GetAppointmentsById(Guid appointmentId){
        return appointmentRepository.GetAppointmentsById(appointmentId);
    }

    public void UpdateStatusAppointment(Guid appointmentId, AppointmentStatus status){
        appointmentRepository.UpdateStatusAppointment(appointmentId, status);
    }

    public List<Appointment> GetAllAppointments(){
        return appointmentRepository.GetAllAppointments();
    }

    public Appointment GetAppointmentsByDoctorIdAndStartTime(Guid doctorId, DateTime StartTime){
        return appointmentRepository.GetAppointmentsByDoctorIdAndStartTime(doctorId, StartTime);
    }
}
