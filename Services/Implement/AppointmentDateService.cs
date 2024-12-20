using System;
using AppointmentHospital.Entity;
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

    public List<Appointment> GetAppointmentsByDoctorId(Guid DoctorId){
        return appointmentRepository.GetAppointmentsByDoctorId(DoctorId);
    }
}
